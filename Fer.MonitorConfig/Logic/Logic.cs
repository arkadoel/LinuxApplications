using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fer.MonitorConfig;

public interface ILogic
{
    List<string> DetectScreens();
    Dictionary<string, string> GetCurrentBrightness();
    string RunCommand(string command, string arguments);
}

internal class Logic : ILogic
{
    public List<string> DetectScreens()
    {
        string output = RunCommand("xrandr", "--verbose");

        string connected = string.Join(Environment.NewLine,
            output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Contains(" connected "))
        );

        if (string.IsNullOrEmpty(connected))
        {
            return new List<string>();
        }

        // Dividir por líneas, y para cada línea, dividir por espacios
        // y tomar la primera palabra.
        var displayNames = connected
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).First())
            .ToList();

        System.Console.WriteLine("Detected screens:");
        System.Console.WriteLine(string.Join('\n', displayNames));

        return displayNames;

    }

    public Dictionary<string, string> GetCurrentBrightness()
    {
        List<string> brillos = new List<string>();
        string xrandrOutput = RunCommand("xrandr", "--verbose");

        if (!string.IsNullOrEmpty(xrandrOutput))
        {
            string[] lines = xrandrOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("Brightness:"))
                {
                    brillos.Add(line.Replace("\tBrightness: ", "").Trim());
                }
            }
        }

        System.Console.WriteLine("Detected Brightness:");
        System.Console.WriteLine(string.Join('\n', brillos));

        var monitores = DetectScreens();

        Dictionary<string, string> myMonitors = new Dictionary<string, string>();
        int indice = 0;
        foreach (var monitor in monitores)
        {
            //Console.WriteLine(monitor);
            if (brillos.Count > indice)
            {
                myMonitors[monitor] = brillos[indice];
            }
            indice++;
        }

        return myMonitors;
        

    }

    public string RunCommand(string command, string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        try
        {
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error executing command: {ex.Message}");
            return null;
        }
    }
}
