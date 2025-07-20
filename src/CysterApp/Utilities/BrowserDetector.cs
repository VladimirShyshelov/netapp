using CysterApp.Factories;
using Microsoft.Win32;

namespace CysterApp.Utilities;

public static class BrowserDetector
{
    public static bool IsInstalled(BrowserType type)
    {
        return TryGetPath(type, out _);
    }

    public static string? GetExecutablePath(BrowserType type)
    {
        return TryGetPath(type, out var p) ? p : null;
    }

    private static bool TryGetPath(BrowserType type, out string? path)
    {
        foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
        {
            if (TryReadAppPath(hive, RegistryView.Registry64, type, out path)) return true;
            if (TryReadAppPath(hive, RegistryView.Registry32, type, out path)) return true;
        }

        path = GetFallbackPath(type);
        return File.Exists(path);
    }

    private static bool TryReadAppPath(RegistryHive hive, RegistryView view,
        BrowserType type, out string? exePath)
    {
        exePath = null;
        using var baseKey = RegistryKey.OpenBaseKey(hive, view);
        if (baseKey == null) return false;

        var subKey = @$"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{GetExeName(type)}";
        using var key = baseKey.OpenSubKey(subKey);
        exePath = key?.GetValue(string.Empty) as string;
        return File.Exists(exePath);
    }

    private static string? GetFallbackPath(BrowserType type)
    {
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        return type switch
        {
            BrowserType.Chrome => Path.Combine(pf, "Google", "Chrome", "Application", "chrome.exe"),
            BrowserType.Edge => Path.Combine(pf86, "Microsoft", "Edge", "Application", "msedge.exe"),
            BrowserType.Firefox => Path.Combine(pf, "Mozilla Firefox", "firefox.exe"),
            _ => null
        };
    }

    private static string GetExeName(BrowserType type)
    {
        return type switch
        {
            BrowserType.Chrome => "chrome.exe",
            BrowserType.Edge => "msedge.exe",
            BrowserType.Firefox => "firefox.exe",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}