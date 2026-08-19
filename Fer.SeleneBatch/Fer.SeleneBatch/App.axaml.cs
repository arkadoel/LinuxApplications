using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Fer.SeleneBatch;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;

            // Si se pasaron archivos como argumento al abrir la app
            if (desktop.Args != null && desktop.Args.Length > 0)
            {
                mainWindow.Loaded += async (s, e) =>
                {
                    if (mainWindow.DataContext is MainViewModel vm)
                    {
                        await vm.ProcessInputPathsAsync(desktop.Args);
                    }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}