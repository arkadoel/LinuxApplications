using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;

namespace Fer.SeleneBatch;

public class MainViewModel : INotifyPropertyChanged
{
    private BatchItem? _selectedFile;
    private string? _selectedScript;
    private bool _isProcessing;

    // Control de cancelación y referencias del proceso
    private CancellationTokenSource? _cts;
    private Process? _currentProcess;

    public Func<Task<IEnumerable<string>>>? OpenVideoPickerFunc { get; set; }

    public ObservableCollection<BatchItem> Files { get; } = new();
    public ObservableCollection<string> AvailableScripts { get; } = new();

    public ICommand AddFilesCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand StartProcessCommand { get; }
    public ICommand CancelProcessCommand { get; }
    public ICommand BrowseScriptCommand { get; }

    public BatchItem? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (_selectedFile != value)
            {
                _selectedFile = value;
                OnPropertyChanged();
                (RemoveFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? SelectedScript
    {
        get => _selectedScript;
        set
        {
            if (_selectedScript != value)
            {
                _selectedScript = value;
                OnPropertyChanged();
                (StartProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (_isProcessing != value)
            {
                _isProcessing = value;
                OnPropertyChanged();
                (StartProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public MainViewModel()
    {
        AddFilesCommand = new RelayCommand(async () => await OnAddFilesAsync());
        RemoveFileCommand = new RelayCommand(OnRemoveFile, CanRemoveFile);
        MoveUpCommand = new RelayCommand(OnMoveUp);
        MoveDownCommand = new RelayCommand(OnMoveDown);

        StartProcessCommand = new RelayCommand(async () => await OnStartProcessAsync(), CanStartProcess);
        CancelProcessCommand = new RelayCommand(OnCancelProcess, CanCancelProcess); // Registrar Cancelar
        BrowseScriptCommand = new RelayCommand(OnBrowseScript);

        AvailableScripts.Add("/home/fer/Documentos/PROYECTOS/LinuxApplications/Fer.SeleneBatch/Fer.SeleneBatch/Scripts/fer2mpgElements.sh");
        if (AvailableScripts.Count > 0)
        {
            SelectedScript = AvailableScripts[0];
        }
    }

    // ==========================================
    // LÓGICA DE PROCESAMIENTO Y CANCELACIÓN
    // ==========================================

    private bool CanStartProcess() => !IsProcessing && !string.IsNullOrEmpty(SelectedScript) && Files.Count > 0;

    private bool CanCancelProcess() => IsProcessing;

    /// <summary>
    /// Cancela el token de cancelación y mata el proceso ffmpeg/bash activo inmediatamente.
    /// </summary>
    private void OnCancelProcess()
    {
        // 1. Notificar al token que se ha solicitado la cancelación
        _cts?.Cancel();

        // 2. Si hay un proceso de sistema ejecutándose en este momento, forzar su cierre
        try
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _currentProcess.Kill(entireProcessTree: true); // Destruye bash y sus subprocesos (ffmpeg)
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SeleneBatch] Error al finalizar el proceso: {ex.Message}");
        }
    }

    private async Task OnStartProcessAsync()
    {
        if (string.IsNullOrEmpty(SelectedScript) || Files.Count == 0) return;

        IsProcessing = true;
        _cts = new CancellationTokenSource();

        try
        {
            int count = 0;
            while (Files.Count > 0 && !_cts.Token.IsCancellationRequested && count < Files.Count)
            {
                var item = Files[count];
                SelectedFile = item;

                // Ejecutamos pasando el token de cancelación
                try
                {
                    await ExecuteScriptForFileAsync(item.FilePath, SelectedScript, item, _cts.Token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al procesar {ex.Message}");
                }

                // Si no se ha solicitado cancelar y el elemento sigue ahí, lo quitamos de la lista
                /*if (!_cts.Token.IsCancellationRequested && Files.Contains(item))
                {
                    Dispatcher.UIThread.Post(() => Files.Remove(item));
                }*/
                count += 1;
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("[SeleneBatch] Procesamiento cancelado por el usuario.");
        }
        finally
        {
            IsProcessing = false;
            SelectedFile = null;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async Task ExecuteScriptForFileAsync(string filePath, string scriptPath, BatchItem item, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"\"{scriptPath}\" \"{filePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        TimeSpan totalDuration = TimeSpan.Zero;
        var durationRegex = new Regex(@"Duration:\s*(\d{2}):(\d{2}):(\d{2})\.(\d{2})", RegexOptions.Compiled);
        var timeRegex = new Regex(@"time=\s*(\d{2}):(\d{2}):(\d{2})\.(\d{2})", RegexOptions.Compiled);

        using (_currentProcess = new Process { StartInfo = startInfo })
        {
            _currentProcess.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data) || cancellationToken.IsCancellationRequested) return;

                if (totalDuration == TimeSpan.Zero)
                {
                    var matchDuration = durationRegex.Match(e.Data);
                    if (matchDuration.Success)
                    {
                        int hours = int.Parse(matchDuration.Groups[1].Value);
                        int minutes = int.Parse(matchDuration.Groups[2].Value);
                        int seconds = int.Parse(matchDuration.Groups[3].Value);
                        totalDuration = new TimeSpan(hours, minutes, seconds);

                        Dispatcher.UIThread.Post(() => item.Duration = totalDuration);
                    }
                }

                var matchTime = timeRegex.Match(e.Data);
                if (matchTime.Success && totalDuration.TotalSeconds > 0)
                {
                    int hours = int.Parse(matchTime.Groups[1].Value);
                    int minutes = int.Parse(matchTime.Groups[2].Value);
                    int seconds = int.Parse(matchTime.Groups[3].Value);
                    var currentTime = new TimeSpan(hours, minutes, seconds);

                    double percentage = (currentTime.TotalSeconds / totalDuration.TotalSeconds) * 100.0;
                    int progress = Math.Clamp((int)Math.Round(percentage), 0, 100);

                    Dispatcher.UIThread.Post(() => item.Progress = progress);
                }
            };

            _currentProcess.Start();
            _currentProcess.BeginErrorReadLine();
            _currentProcess.BeginOutputReadLine();

            try
            {
                // Espera asíncrona permitiendo ser cancelada
                await _currentProcess.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Si se solicita cancelar mientras espera, forzamos la muerte del árbol de procesos
                if (!_currentProcess.HasExited)
                {
                    _currentProcess.Kill(entireProcessTree: true);
                }
                throw;
            }
            finally
            {
                _currentProcess = null;
            }
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            Dispatcher.UIThread.Post(() => item.Progress = 100);
        }
    }

    // ==========================================
    // GESTIÓN DEL LISTADO
    // ==========================================

    private async Task OnAddFilesAsync()
    {
        if (OpenVideoPickerFunc == null) return;

        var selectedPaths = await OpenVideoPickerFunc();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var path in selectedPaths)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);

                    Files.Add(new BatchItem
                    {
                        FilePath = path,
                        FileSizeBytes = fileInfo.Length, // Leemos el tamaño real en bytes
                        Duration = TimeSpan.Zero,
                        Progress = 0
                    });
                }
            }
            (StartProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
        });
    }

    private bool CanRemoveFile() => SelectedFile != null;

    private void OnRemoveFile()
    {
        if (SelectedFile != null)
        {
            Files.Remove(SelectedFile);
            SelectedFile = null;
            (StartProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    private void OnMoveUp()
    {
        if (SelectedFile == null) return;

        int currentIndex = Files.IndexOf(SelectedFile);
        if (currentIndex > 0)
        {
            Files.Move(currentIndex, currentIndex - 1);
        }
    }

    private void OnMoveDown()
    {
        if (SelectedFile == null) return;

        int currentIndex = Files.IndexOf(SelectedFile);
        if (currentIndex >= 0 && currentIndex < Files.Count - 1)
        {
            Files.Move(currentIndex, currentIndex + 1);
        }
    }

    private void OnBrowseScript()
    {
        // Selector de scripts pendiente
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}