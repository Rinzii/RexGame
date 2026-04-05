using Rex.Shared.Net;
using Rex.Shared.Timing;

namespace Content.Shared;

public static class ContentGameInfo
{
    public const string GameName = "RexGame";
    public const string SharedProject = "Content.Shared";
    public const string ClientProject = "Content.Client";
    public const string ServerProject = "Content.Server";
    public const string DefaultHost = "127.0.0.1";

    public static IReadOnlyList<NetMode> SupportedClientModes { get; } =
    [
        NetMode.Standalone,
        NetMode.Client,
        NetMode.ListenServer
    ];

    public static ContentSessionSettings CreateDefaultSessionSettings() =>
        new(
            ProtocolConstants.DefaultPort,
            ProtocolConstants.DefaultTickRate,
            ProtocolConstants.DefaultMaxPlayers);
}

public readonly record struct ContentSessionSettings(int Port, int TickRate, int MaxPlayers)
{
    public TickClock CreateClock() => new(TickRate);
}
