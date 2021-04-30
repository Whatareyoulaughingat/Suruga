using Newtonsoft.Json;

namespace Suruga.Lavalink
{
    public sealed class VoiceStateUpdatePayload
    {
        public VoiceStateUpdatePayload(ulong guildId, ulong? channelId, bool isSelfMuted = false, bool isSelfDeafened = false)
        {
            GuildId = guildId;
            ChannelId = channelId;
            IsSelfMuted = isSelfMuted;
            IsSelfDeafened = isSelfDeafened;
        }

        [JsonRequired]
        [JsonProperty("guild_id")]
        public ulong GuildId { get; }

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Include)]
        public ulong? ChannelId { get; }

        [JsonRequired]
        [JsonProperty("self_mute")]
        public bool IsSelfMuted { get; }

        [JsonRequired]
        [JsonProperty("self_deaf")]
        public bool IsSelfDeafened { get; }
    }
}
