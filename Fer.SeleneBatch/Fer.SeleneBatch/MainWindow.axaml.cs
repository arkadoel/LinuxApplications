using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fer.SeleneBatch;

public partial class MainWindow : Window
{
    private static readonly string[] SupportedExtensions = 
    { ".mp4", ".mkv", ".avi", ".mov", ".webm", ".flv", ".wmv", ".m4v", ".mpg", ".mpeg", ".ts", ".m2ts" };

    public MainWindow()
    {
        InitializeComponent();
        this.Title = $"{Program.APP_NAME} {Program.APP_VERSION}";

        var vm = new MainViewModel();
        vm.OpenVideoPickerFunc = OpenVideoFilesAsync;
        this.DataContext = vm;

        // Escuchar atajo de teclado Ctrl + V
        this.KeyDown += OnWindowKeyDown;
    }

    #region Portapapeles (Botón y Ctrl + V)

    private async void OnPasteButtonClick(object? sender, RoutedEventArgs e)
    {
        await ProcessClipboardFilesAsync();
    }

    private async void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.V)
        {
            await ProcessClipboardFilesAsync();
        }
    }

    private async Task ProcessClipboardFilesAsync()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null) return;

        var paths = new List<string>();

        string? text = await clipboard.GetTextAsync();
        if (!string.IsNullOrEmpty(text))
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string clean = line.Trim();
                
                if (clean.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(clean, UriKind.Absolute, out Uri? uri))
                    {
                        clean = uri.LocalPath;
                    }
                }

                if (Path.IsPathRooted(clean))
                {
                    if (Directory.Exists(clean))
                    {
                        paths.AddRange(ScanFolderForVideos(clean));
                    }
                    else if (File.Exists(clean))
                    {
                        paths.Add(clean);
                    }
                }
            }
        }

        if (paths.Count > 0 && DataContext is MainViewModel vm)
        {
            await vm.ProcessInputPathsAsync(paths);
        }
    }

    private IEnumerable<string> ScanFolderForVideos(string folderPath)
    {
        var foundFiles = new List<string>();
        try
        {
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file);
                if (SupportedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                {
                    foundFiles.Add(file);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] No se pudo leer la carpeta {folderPath}: {ex.Message}");
        }

        return foundFiles;
    }

    #endregion

    #region Picker Manual de Archivos

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

    #endregion
}