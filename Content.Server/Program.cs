using Content.Shared;
using Rex.Server.Startup;

namespace Content.Server;

internal static class Program
{
    private static int Main(string[] args)
    {
        return GameServerStart.Start(args, ContentGameInfo.CreateServerStartDefinition());
    }
}
