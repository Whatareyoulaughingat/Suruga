using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;

namespace Suruga.Extensions
{
    public static class DiscordClientExtensions
    {
        public static IWebSocketClient GetWebSocketClient(this DiscordClient client)
        {
            return typeof(DiscordClient)
                .GetField("_webSocketClient", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(client) as IWebSocketClient;
        }

        public static string GetSessionId(this DiscordVoiceState voiceState)
        {
            return typeof(DiscordVoiceState)
                .GetProperty("SessionId", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(voiceState) as string;
        }

        public static string GetVoiceToken(this VoiceServerUpdateEventArgs voiceServerUpdateEvent)
        {
            return typeof(VoiceServerUpdateEventArgs)
                .GetProperty("VoiceToken", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(voiceServerUpdateEvent) as string;
        }
    }
}
