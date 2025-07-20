using System.Diagnostics;
using System.Runtime.InteropServices;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace CysterApp.Services;

public sealed class ScreenRecorder : IAsyncDisposable
{
    private IConversion? _conversion;
    private CancellationTokenSource? _cts;
    private Task<IConversionResult>? _recordingTask;

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    public async Task StartAsync(string outputPath, int fps = 30)
    {
        if (_recordingTask != null)
            throw new InvalidOperationException("ScreenRecorder already started.");


        var tmpDir = Path.Combine(Path.GetTempPath(), "ffmpeg");
        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

        if (!File.Exists(Path.Combine(tmpDir, exeName)))
        {
            Directory.CreateDirectory(tmpDir);
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, tmpDir);
        }

        FFmpeg.SetExecutablesPath(tmpDir);

        var grabArgs =
            $"-f gdigrab -framerate {fps} -rtbufsize 50M -draw_mouse 1 -i desktop";

        _cts = new CancellationTokenSource();
        _conversion = FFmpeg.Conversions.New()
            .AddParameter(grabArgs, ParameterPosition.PreInput)
            .AddParameter("-c:v libx264 -preset ultrafast -pix_fmt yuv420p")
            .AddParameter("-vf scale=trunc(iw/2)*2:trunc(ih/2)*2")
            .SetOutput(outputPath);

        _recordingTask = _conversion.Start(_cts.Token);
    }

    public async Task StopAsync(TimeSpan? gracePeriod = null)
    {
        if (_recordingTask == null)
            return;

        if (gracePeriod is { } t && t > TimeSpan.Zero)
            await Task.Delay(t);

        _cts?.Cancel();

        try
        {
            await _recordingTask;
        }
        catch (OperationCanceledException)
        {
            /* ignore */
        }

        KillFfmpegProcesses();

        _cts = null;
        _conversion = null;
        _recordingTask = null;
    }

    private static void KillFfmpegProcesses()
    {
        foreach (var p in Process.GetProcessesByName("ffmpeg"))
            try
            {
                p.Kill(true);
            }
            catch
            {
                /* ignore */
            }
    }
}