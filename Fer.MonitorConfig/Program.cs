using Avalonia;
using Fer.MonitorConfig;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fer.MonitorConfig;

class Program
{
    internal static string APP_VERSION = "1.0.1";
    internal const string APP_NAME = "Monitor Config";
    internal const string ICONO = "./media/LOGO.png";

    internal static ServiceProvider? ServiceProvider;

    [STAThread]
    public static void Main(string[] args)
    {        
        ServiceProvider = new ServiceCollection()
            .AddSingleton<ILogic>(new Logic())
            .BuildServiceProvider();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    }

    // Avalonia configuration, don"t remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}