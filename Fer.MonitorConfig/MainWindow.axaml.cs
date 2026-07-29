using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Fer.MonitorConfig;

public partial class MainWindow : Window
{
    private readonly ILogic Logic;

    public MainWindow()
    {
        Logic = Program.ServiceProvider!.GetRequiredService<ILogic>();
        
        InitializeComponent();
        Program.APP_VERSION = Logic.GetAppVersion();
        this.Title = $"{Program.APP_NAME} {Program.APP_VERSION}";
        this.stkMonitores.IsVisible = false;

        var brillos = Logic.GetCurrentBrightness();

        this.stkMonitores.Children.Clear();

        foreach (var monitor in brillos)
        {
            var label = new TextBlock()
            {
                Text = monitor.Key,
                FontWeight = Avalonia.Media.FontWeight.ExtraBold,
                FontFamily = Avalonia.Media.FontFamily.Parse("Verdana")
            };
            stkMonitores.Children.Add(label);

            double brilloDetectado = Convert.ToDouble(monitor.Value);
            System.Console.WriteLine("Brillo detectado " + brilloDetectado.ToString("F2"));
            /*if (brilloDetectado >= 100d)
            {
                brilloDetectado = 99d;
            }*/
            var brilloActual = Convert.ToInt32(brilloDetectado);
            brilloActual = Convert.ToInt32(99d); //fix: iniciar con este brillo siempre
            System.Console.WriteLine("SET Brillo inicial a: " + brilloActual.ToString("F2"));
            SetBrightness(monitor.Key, brilloActual);
            System.Console.WriteLine("Brillo actual " + brilloActual.ToString());

            var slider = new Slider()
            {
                Maximum = 101,
                Minimum = 1,
                Value = brilloActual,
                Name = monitor.Key
            };
            slider.ValueChanged += SliderValueChanged;
            stkMonitores.Children.Add(slider);
        }

        this.stkMonitores.IsVisible = true;
    }

    private void SliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        var slider = sender as Slider;
        string monitor = slider!.Name!;
        double brillo = slider!.Value / 100;

        SetBrightness(monitor, brillo);
    }

    private void SetBrightness(string monitor, double brillo)
    {
        System.Console.WriteLine("Set brightness to" + brillo.ToString("F2", CultureInfo.InvariantCulture));
        if (brillo >= 0.99)
        {
            brillo = 0.99;
        }
        //xrandr --output eDP-1 --brightness 0.99

        string args = $"--output {monitor} --brightness {brillo.ToString("F2", CultureInfo.InvariantCulture)}";
        System.Console.WriteLine($"xrandr {args}");
        Logic.RunCommand("xrandr", args);
    }
}