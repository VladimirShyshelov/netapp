using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CysterApp.Utilities;

internal static class SelfDeleter
{
    public static void SelfDelete()
    {
        var exe = Environment.ProcessPath!;
        var log = Path.Combine(Path.GetDirectoryName(exe)!, "automation.log");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            RunWindowsCleaner(exe, log);
        else
            RunUnixCleaner(exe, log);

        Environment.Exit(0);
    }


    private static void RunWindowsCleaner(string exe, string log)
    {
        var bat = Path.Combine(Path.GetTempPath(), $"del_{Guid.NewGuid()}.bat");

        File.WriteAllText(bat, $@"
@echo off
setlocal
set EXE=""{exe}""
set LOG=""{log}""

:retry
ping 127.0.0.1 -n 3 >nul
del /f /q %EXE% >nul 2>nul
if exist %EXE% goto retry

if exist %LOG% del /f /q %LOG% >nul 2>nul
del /f /q ""%~f0""
");

        var psi = new ProcessStartInfo("cmd", $"/c \"{bat}\"")
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        Process.Start(psi);
    }

    private static void RunUnixCleaner(string exe, string log)
    {
        var sh = Path.Combine(Path.GetTempPath(), $"del_{Guid.NewGuid()}.sh");
        File.WriteAllText(sh, $@"
#!/usr/bin/env bash
set -e

EXE=""{exe}""
LOG=""{log}""

until rm -f ""$EXE"" 2>/dev/null; do
  sleep 2
done

[ -f ""$LOG"" ] && rm -f ""$LOG""
rm -- ""$0""
");
        Process.Start("/bin/bash", sh);
    }
}