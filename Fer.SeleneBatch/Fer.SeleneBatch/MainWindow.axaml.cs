using Avalonia.Controls;

namespace Fer.SeleneBatch;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Title = $"{Program.APP_NAME} {Program.APP_VERSION}";
    }
}