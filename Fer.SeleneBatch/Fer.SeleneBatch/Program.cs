using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;
using Fer.SeleneBatch.Core;

namespace Fer.SeleneBatch;

class Program
{
    internal const string APP_VERSION = "0.1.0";
    internal const string APP_NAME = "Selene Batch";
    internal static ServiceProvider? ServiceProvider;

    [STAThread]
    public static void Main(string[] args)
    {
        ServiceProvider = new ServiceCollection()
            //.AddSingleton<ILogic>(new Logic())
            .BuildServiceProvider();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
