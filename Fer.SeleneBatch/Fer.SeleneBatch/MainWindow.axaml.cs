using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fer.SeleneBatch;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Title = $"{Program.APP_NAME} {Program.APP_VERSION}";

        // Asignamos explícitamente el DataContext y registramos la función del picker
        var vm = new MainViewModel();
        vm.OpenVideoPickerFunc = OpenVideoFilesAsync;
        this.DataContext = vm;
    }

    private async Task<IEnumerable<string>> OpenVideoFilesAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return Enumerable.Empty<string>();

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleccionar archivos de vídeo",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Archivos de vídeo")
                {
                    Patterns = new[] { "*.mp4", "*.mkv", "*.avi", "*.mov", "*.webm", "*.flv" }
                },
                FilePickerFileTypes.All
            }
        });

        var paths = new List<string>();
        foreach (var file in files)
        {
            var localPath = file.TryGetLocalPath();
            if (!string.IsNullOrEmpty(localPath))
            {
                paths.Add(localPath);
            }
            else if (file.Path != null)
            {
                paths.Add(file.Path.LocalPath);
            }
        }

        return paths;
    }
}