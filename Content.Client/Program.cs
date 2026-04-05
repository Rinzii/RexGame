using Content.Shared;
using Rex.Client.Startup;

namespace Content.Client;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        return GameClientStart.Start(args, ContentGameInfo.CreateClientStartDefinition());
    }
}
