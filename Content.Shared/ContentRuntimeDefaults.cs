using Rex.Shared.Net;
using Rex.Shared.Startup;
using Rex.Shared.Timing;

namespace Content.Shared;

/// <summary>
/// Defines game specific startup values that the engine consumes.
/// </summary>
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

    /// <summary>
    /// Creates the default shared session settings for the game.
    /// </summary>
    public static ContentSessionSettings CreateDefaultSessionSettings() =>
        new(
            ProtocolConstants.DefaultPort,
            ProtocolConstants.DefaultTickRate,
            ProtocolConstants.DefaultMaxPlayers);

    /// <summary>
    /// Builds the game definition for client startup.
    /// </summary>
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

    /// <summary>
    /// Builds the game definition for server startup.
    /// </summary>
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

/// <summary>
/// Groups default session settings for content startup.
/// </summary>
public readonly record struct ContentSessionSettings(int Port, int TickRate, int MaxPlayers)
{
    public TickClock CreateClock() => new(TickRate);
}
