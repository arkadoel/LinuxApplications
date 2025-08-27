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
        InitializeComponent();
        this.Title = $"{Program.APP_NAME} {Program.APP_VERSION}";
        this.stkMonitores.IsVisible = false;

        Logic = Program.ServiceProvider!.GetRequiredService<ILogic>();
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

            var brilloActual = Convert.ToInt32(Convert.ToDouble(monitor.Value));
            System.Console.WriteLine("Brillo actual " + brilloActual.ToString());

            var slider = new Slider()
            {
                Maximum = 100,
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
        //xrandr --output eDP-1 --brightness 0.99

        string args = $"--output {monitor} --brightness {brillo.ToString("F2", CultureInfo.InvariantCulture)}";
        System.Console.WriteLine($"xrandr {args}");
        Logic.RunCommand("xrandr", args);
    }
}