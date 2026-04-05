using Content.Shared;
using Microsoft.Extensions.Logging;
using Rex.Server.Net;

namespace Content.Server;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (!ContentServerOptions.TryParse(args, out var options, out var error))
        {
            Console.Error.WriteLine(error);
            return 1;
        }

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var host = new ContentServerHost(options, loggerFactory.CreateLogger<ContentServerHost>());
        return host.Run();
    }
}

internal sealed record ContentServerOptions(int Port, int TickRate, int MaxPlayers)
{
    public static bool TryParse(
        IReadOnlyList<string> args,
        out ContentServerOptions options,
        out string? error)
    {
        var defaults = ContentGameInfo.CreateDefaultSessionSettings();
        var port = defaults.Port;
        var tickRate = defaults.TickRate;
        var maxPlayers = defaults.MaxPlayers;

        using var enumerator = args.GetEnumerator();
        while (enumerator.MoveNext())
        {
            switch (enumerator.Current)
            {
                case "--port" when !enumerator.MoveNext() || !int.TryParse(enumerator.Current, out port):
                    options = default!;
                    error = "Missing or invalid value for --port.";
                    return false;
                case "--tick-rate" when !enumerator.MoveNext() || !int.TryParse(enumerator.Current, out tickRate):
                    options = default!;
                    error = "Missing or invalid value for --tick-rate.";
                    return false;
                case "--max-players" when !enumerator.MoveNext() || !int.TryParse(enumerator.Current, out maxPlayers):
                    options = default!;
                    error = "Missing or invalid value for --max-players.";
                    return false;
            }
        }

        options = new ContentServerOptions(port, tickRate, maxPlayers);
        error = null;
        return true;
    }
}

internal sealed partial class ContentServerHost(ContentServerOptions options, ILogger<ContentServerHost> logger)
{
    public int Run()
    {
        var session = new ContentSessionSettings(options.Port, options.TickRate, options.MaxPlayers);
        var clock = session.CreateClock();
        clock.IncrementTick();
        var engineAssemblyName = typeof(RemoteServerNetChannel).Assembly.GetName().Name ?? "Rex.Server";

        LogServerBootstrapStarting(
            ContentGameInfo.GameName,
            ContentGameInfo.ServerProject);
        LogEngineRuntimeAssembly(engineAssemblyName);
        LogServerSettings(
            session.Port,
            session.TickRate,
            session.MaxPlayers);
        LogServerBootstrapCompleted(
            ContentGameInfo.GameName,
            clock.CurrentTick);
        return 0;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "{Game} dedicated server bootstrap starting from {Project}.")]
    private partial void LogServerBootstrapStarting(string game, string project);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Authoritative game code lives in Content.* while engine server runtime code comes from {Assembly}.")]
    private partial void LogEngineRuntimeAssembly(string assembly);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "Server settings: UDP {Port}, {TickRate} Hz, max players {MaxPlayers}.")]
    private partial void LogServerSettings(int port, int tickRate, int maxPlayers);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "{Game} server bootstrap completed at simulation tick {Tick}.")]
    private partial void LogServerBootstrapCompleted(string game, uint tick);
}
