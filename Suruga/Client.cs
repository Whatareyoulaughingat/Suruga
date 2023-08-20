using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Suruga.Logging;
using System.Net;
using System.Reflection;

namespace Suruga;

internal sealed class Client
{
    private readonly IConfiguration _configuration = Program.Host.Services.GetRequiredService<IConfiguration>();

    private readonly DiscordClient _client = Program.Host.Services.GetRequiredService<DiscordClient>();

    private readonly Logger _logger = Program.Host.Services.GetRequiredService<Logger>();

    internal async Task RunAsync()
    {
        try
        {
            _client.Ready += OnReady;

            await _client.ConnectAsync(new DiscordActivity
            {
                Name = _configuration.GetRequiredSection("Activity")["Name"] ?? string.Empty,
                ActivityType = Enum.TryParse(_configuration.GetRequiredSection("Activity")["Type"], out ActivityType activityType) is true ? activityType : default,

            }, Enum.TryParse(_configuration.GetRequiredSection("Status")["Status"], out UserStatus status) ? status : UserStatus.Online);
        }
        catch (DisCatSharpException ex)
        {
            string errorMessage = ex switch
            {
                UnauthorizedException => "Failed to start. Unable to get proper permissions.",
                BadRequestException => "Failed to start. A malformed request was detected. Try running the bot again.",
                ServerErrorException => "Failed to start. An internal server error has occurred. Try running the bot again " +
                                        "or check for Discord's server status at https://discordstatus.com/",
                _ => "An unknown error occurred while starting the bot."
            };
            
            _logger.LogCritical(errorMessage, ex);
            await ShutdownAsync();
        }

        await Task.Delay(Timeout.Infinite);
        await ShutdownAsync();
    }

    private Task OnReady(DiscordClient client, ReadyEventArgs readyArgs)
    {
        ApplicationCommandsExtension commandExt = client.UseApplicationCommands(new(Program.Host.Services));

#if DEBUG
        commandExt.RegisterGuildCommands(Assembly.GetExecutingAssembly(), 1122190575232352306);
#else
        commandExt.RegisterGlobalCommands(Assembly.GetExecutingAssembly());
#endif

        Thread lavalinkConnectionThread = new(async () =>
        {
            LavalinkExtension lavalink = client.UseLavalink();

            (string IpAddress, int Port) = _configuration
                .GetRequiredSection("Lavalink")["Endpoint"]
                ?.Split(':') is { Length: 2 } endpointParts
                ? (endpointParts[0], int.Parse(endpointParts[1]))
                : ("127.0.0.1", 2333);

            (string IpAddress, int Port)? proxyEndpoint = _configuration
                .GetRequiredSection("Lavalink")["ProxyEndpoint"]
                ?.Split(':') is { Length: 2 } proxyEndpointParts
                ? (proxyEndpointParts[0], int.Parse(proxyEndpointParts[1]))
                : null;

            try
            {
                await lavalink.ConnectAsync(new LavalinkConfiguration
                {
                    DefaultVolume = _configuration.GetRequiredSection("Lavalink").GetValue<int>("Volume") is < 0 or > 2000 ? 70 : _configuration.GetRequiredSection("Lavalink").GetValue<int>("Volume"),
                    Password = _configuration.GetRequiredSection("Lavalink")["Password"] ?? "youshallnotpass",
                    RestEndpoint = new(IpAddress, Port),
                    SocketEndpoint = new(IpAddress, Port),
                    Proxy = proxyEndpoint is { IpAddress: string ip, Port: int port } ? new WebProxy(ip, port) : null!,
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Launch Lavalink first before running the bot.", ex);
                await ShutdownAsync();
            }
        });

        lavalinkConnectionThread.Start();

        _logger.LogInformation("Initialization complete.");
        return Task.CompletedTask;
    }

    private async Task ShutdownAsync()
    {
        _client.Ready -= OnReady;
        await _client.DisconnectAsync();
        _client.Dispose();

        await Program.Host.StopAsync();
        Program.Host.Dispose();

        Environment.Exit(0);
    }
}
