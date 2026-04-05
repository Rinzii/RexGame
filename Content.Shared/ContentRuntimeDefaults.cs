using Rex.Shared.Net;
using Rex.Shared.Startup;
using Rex.Shared.Timing;

namespace Content.Shared;

/// <summary>Game constants and factories the engine reads during bootstrap.</summary>
public static class ContentGameInfo
{
    public const string GameName = "RexGame";
    public const string SharedProject = "Content.Shared";
    public const string ClientProject = "Content.Client";
    public const string ServerProject = "Content.Server";
    public const string DedicatedServerName = "RexGame Dedicated Server";
    public const string DefaultHost = "127.0.0.1";
    public const string DefaultWindowTitle = "RexGame";
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;
    public const string ListenServerReadyLine = "REXGAME_SERVER_READY";

    public static IReadOnlyList<NetMode> SupportedClientModes { get; } =
    [
        NetMode.Standalone,
        NetMode.Client,
        NetMode.ListenServer
    ];

    /// <summary>Default port, tick rate and player cap from <see cref="ProtocolConstants"/>.</summary>
    public static ContentSessionSettings CreateDefaultSessionSettings() =>
        new(
            ProtocolConstants.DefaultPort,
            ProtocolConstants.DefaultTickRate,
            ProtocolConstants.DefaultMaxPlayers);

    /// <summary>Builds <see cref="GameClientStartDefinition"/> with RexGame window defaults and local listen server wiring.</summary>
    public static GameClientStartDefinition CreateClientStartDefinition()
    {
        var defaults = CreateDefaultSessionSettings();
        return new GameClientStartDefinition(
            new GameRuntimeIdentity(GameName, SharedProject, ClientProject, ServerProject),
            DefaultHost,
            defaults.Port,
            defaults.TickRate,
            new GameWindowDefinition(DefaultWindowTitle, DefaultWindowWidth, DefaultWindowHeight),
            new ListenServerDefinition("REX_CONTENT_SERVER_DLL", "Content.Server.dll", ListenServerReadyLine, DefaultHost));
    }

    /// <summary>Builds <see cref="GameServerStartDefinition"/> with RexGame dedicated name and ready line.</summary>
    public static GameServerStartDefinition CreateServerStartDefinition()
    {
        var defaults = CreateDefaultSessionSettings();
        return new GameServerStartDefinition(
            new GameRuntimeIdentity(GameName, SharedProject, ClientProject, ServerProject),
            DedicatedServerName,
            ListenServerReadyLine,
            defaults.Port,
            defaults.TickRate,
            defaults.MaxPlayers);
    }
}

/// <summary>Port, tick rate and player cap baseline before CLI overrides.</summary>
/// <param name="Port">Baseline UDP port.</param>
/// <param name="TickRate">Fixed simulation rate in Hz.</param>
/// <param name="MaxPlayers">Baseline session capacity.</param>
public readonly record struct ContentSessionSettings(int Port, int TickRate, int MaxPlayers)
{
    public TickClock CreateClock() => new(TickRate);
}
