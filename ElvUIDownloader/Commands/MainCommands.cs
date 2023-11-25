using System.Windows;
using System.Windows.Threading;

namespace ElvUIDownloader.Commands;

public static class MainCommands
{
    public static AsyncCommand ApplicationClose
    {
        get { return new AsyncCommand(async (r) => { Application.Current.Shutdown(); }); }
    }

    public static Dispatcher ReturnToUI() => Application.Current.Dispatcher;
}