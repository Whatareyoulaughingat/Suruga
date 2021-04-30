using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lavalink4NET;
using Lavalink4NET.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Suruga.Extensions;

namespace Suruga.Lavalink.Wrappers
{
    public class DiscordShardedClientWrapper : IDisposable
    {
        private readonly DiscordShardedClient shardedClient;

        public DiscordShardedClientWrapper(DiscordShardedClient shardedclient)
        {
            shardedClient = shardedclient;

            shardedClient.VoiceStateUpdated += OnVoiceStateUpdated;
            shardedClient.VoiceServerUpdated += OnVoiceServerUpdated;
        }

#nullable enable
        public event AsyncEventHandler<VoiceServer>? VoiceServerUpdated;

        public event AsyncEventHandler<Lavalink4NET.Events.VoiceStateUpdateEventArgs>? VoiceStateUpdated;
#nullable disable

        public int ShardCount
        {
            get { return shardedClient.ShardClients.Count; }
        }

        public DiscordUser CurrentUser
        {
            get { return shardedClient.CurrentUser; }
        }

        public async Task<IEnumerable<ulong>> GetChannelUsersAsync(ulong guildId, ulong voiceChannelId)
        {
            DiscordGuild guild = await GetClient(guildId).GetGuildAsync(guildId);
            DiscordChannel channel = guild.GetChannel(voiceChannelId);

            return channel.Users.Select(x => x.Id);
        }

        public async Task InitializeAsync()
        {
            while (CurrentUser is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                if (DateTimeOffset.UtcNow - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(10))
                {
                    throw new TimeoutException("The initialization of the discord client has exceeded the timeout limit of 10 seconds.");
                }
            }
        }

        public async Task SendVoiceUpdateAsync(ulong guildId, ulong? voiceChannelId, bool selfMuted = false, bool selfDeafeaned = false)
        {
            JObject payload = new();
            VoiceStateUpdatePayload data = new(guildId, voiceChannelId, selfMuted, selfDeafeaned);

            payload.Add("op", 4);
            payload.Add("d", JObject.FromObject(data));

            string message = JsonConvert.SerializeObject(payload, Formatting.None);
            await GetClient(guildId).GetWebSocketClient().SendMessageAsync(message);
        }

        public void Dispose()
        {
            shardedClient.VoiceStateUpdated -= OnVoiceStateUpdated;
            shardedClient.VoiceServerUpdated -= OnVoiceServerUpdated;

            GC.SuppressFinalize(this);
        }

        public DiscordClient GetClient(ulong guildId)
            => shardedClient.GetShard(guildId);

        private Task OnVoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs voiceServer)
        {
            VoiceServer args = new(voiceServer.Guild.Id, voiceServer.GetVoiceToken(), voiceServer.Endpoint);
            return VoiceServerUpdated.InvokeAsync(this, args);
        }

        private Task OnVoiceStateUpdated(DiscordClient client, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs eventArgs)
        {
            // Session id is the same as the resume key so DSharpPlus should be able to give us the session key in either before or after voice state.
            var sessionId = eventArgs.Before?.GetSessionId() ?? eventArgs.After.GetSessionId();

            // create voice state
            VoiceState voiceState = new(
                voiceChannelId: eventArgs.After?.Channel?.Id,
                guildId: eventArgs.Guild.Id,
                voiceSessionId: sessionId);

            // invoke event
            return VoiceStateUpdated.InvokeAsync(
                this,
                new Lavalink4NET.Events.VoiceStateUpdateEventArgs(eventArgs.User.Id, voiceState));
        }
    }
}
