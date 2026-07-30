using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Fer.SeleneBatch;

public class BatchItem : INotifyPropertyChanged
{
    private string _filePath = string.Empty;
    private long _fileSizeBytes;
    private TimeSpan _duration;
    private double _progress;

    /// <summary>
    /// Ruta completa del archivo (ej: /home/usuario/videos/video1.mp4)
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath != value)
            {
                _filePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FileName));
            }
        }
    }

    /// <summary>
    /// Devuelve solo el nombre del archivo
    /// </summary>
    public string FileName => System.IO.Path.GetFileName(FilePath);

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long FileSizeBytes
    {
        get => _fileSizeBytes;
        set
        {
            if (_fileSizeBytes != value)
            {
                _fileSizeBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FileSizeText));
            }
        }
    }

    /// <summary>
    /// Devuelve el tamaño en formato legible (ej: 45.2 MB, 1.20 GB)
    /// </summary>
    public string FileSizeText
    {
        get
        {
            if (_fileSizeBytes <= 0) return "0 B";

            string[] sufijos = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double bytes = _fileSizeBytes;

            while (bytes >= 1024 && i < sufijos.Length - 1)
            {
                bytes /= 1024;
                i++;
            }

            return $"{bytes:0.##} {sufijos[i]}";
        }
    }

    /// <summary>
    /// Duración del archivo de vídeo
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (_duration != value)
            {
                _duration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DurationText));
            }
        }
    }

    /// <summary>
    /// Devuelve la duración en formato HH:mm:ss
    /// </summary>
    public string DurationText => Duration.ToString(@"hh\:mm\:ss");

    /// <summary>
    /// Progreso actual del procesamiento (0 a 100)
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            if (Math.Abs(_progress - value) > 0.001)
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}