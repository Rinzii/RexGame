using Content.Shared;
using Microsoft.Extensions.Logging;
using Rex.Client.Net;
using Rex.Shared.Net;

namespace Content.Client;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (!ContentClientOptions.TryParse(args, out var options, out var error))
        {
            Console.Error.WriteLine(error);
            return 1;
        }

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var host = new ContentClientHost(options, loggerFactory.CreateLogger<ContentClientHost>());
        return host.Run();
    }
}

internal sealed record ContentClientOptions(bool Headless, NetMode Mode, string Host, int Port)
{
    public static bool TryParse(
        IReadOnlyList<string> args,
        out ContentClientOptions options,
        out string? error)
    {
        var headless = false;
        var listen = false;
        var standalone = false;
        string? host = null;
        var port = ProtocolConstants.DefaultPort;

        using var enumerator = args.GetEnumerator();
        while (enumerator.MoveNext())
        {
            switch (enumerator.Current)
            {
                case "--headless":
                    headless = true;
                    break;
                case "--listen":
                    listen = true;
                    break;
                case "--standalone":
                    standalone = true;
                    break;
                case "--connect" when !enumerator.MoveNext():
                    options = default!;
                    error = "Missing value for --connect.";
                    return false;
                case "--connect":
                    host = enumerator.Current;
                    break;
                case "--port" when !enumerator.MoveNext() || !int.TryParse(enumerator.Current, out port):
                    options = default!;
                    error = "Missing or invalid value for --port.";
                    return false;
            }
        }

        var mode = standalone
            ? NetMode.Standalone
            : listen
                ? NetMode.ListenServer
                : host is null
                    ? NetMode.Standalone
                    : NetMode.Client;

        options = new ContentClientOptions(headless, mode, host ?? ContentGameInfo.DefaultHost, port);
        error = null;
        return true;
    }
}

internal sealed partial class ContentClientHost(ContentClientOptions options, ILogger<ContentClientHost> logger)
{
    public int Run()
    {
        var session = ContentGameInfo.CreateDefaultSessionSettings() with { Port = options.Port };
        var clock = session.CreateClock();
        clock.IncrementTick();
        var engineAssemblyName = typeof(RemoteClientNetChannel).Assembly.GetName().Name ?? "Rex.Client";

        LogClientBootstrapStarting(
            ContentGameInfo.GameName,
            options.Mode,
            ContentGameInfo.ClientProject);
        LogEngineRuntimeAssembly(engineAssemblyName);
        LogSharedRuntimeDefaults(
            ContentGameInfo.SharedProject,
            session.TickRate,
            session.Port);

        if (options.Mode is NetMode.Client or NetMode.ListenServer)
        {
            LogRemoteEndpoint(
                options.Host,
                session.Port,
                ProtocolConstants.ConnectionKey);
        }
        else
        {
            LogStandaloneSelected();
        }

        if (options.Headless)
        {
            LogHeadlessEnabled();
        }

        LogClientBootstrapCompleted(
            ContentGameInfo.GameName,
            clock.CurrentTick);
        return 0;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "{Game} client bootstrap starting in {Mode} mode from {Project}.")]
    private partial void LogClientBootstrapStarting(string game, NetMode mode, string project);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Game code lives in Content.* while engine runtime code is supplied by {Assembly}.")]
    private partial void LogEngineRuntimeAssembly(string assembly);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "Shared game contracts come from {SharedProject}; default runtime is {TickRate} Hz on port {Port}.")]
    private partial void LogSharedRuntimeDefaults(string sharedProject, int tickRate, int port);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "Configured remote endpoint: {Host}:{Port} using engine connection key {ConnectionKey}.")]
    private partial void LogRemoteEndpoint(string host, int port, string connectionKey);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information,
        Message = "Standalone bootstrap selected; no remote connection is required.")]
    private partial void LogStandaloneSelected();

    [LoggerMessage(EventId = 6, Level = LogLevel.Information,
        Message = "Headless client mode is enabled for content-side bootstrap.")]
    private partial void LogHeadlessEnabled();

    [LoggerMessage(EventId = 7, Level = LogLevel.Information,
        Message = "{Game} client bootstrap completed at simulation tick {Tick}.")]
    private partial void LogClientBootstrapCompleted(string game, uint tick);
}
