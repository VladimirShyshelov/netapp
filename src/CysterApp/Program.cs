using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using CysterApp.Config;
using CysterApp.Factories;
using CysterApp.Hcp.Clients;
using CysterApp.Services;
using CysterApp.UI;
using CysterApp.Utilities;
using CysterApp.WebBrowser;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Serilog.Debugging;
using SharpHook;
using SharpHook.Data;
using SharpHook.Native;

namespace CysterApp;

internal class Program
{
    private static readonly string BasePath = Path.GetDirectoryName(Environment.ProcessPath)!;
    private static readonly string DownloadedDir = Path.Combine(BasePath, "downloaded");
    private static readonly string VideoDir = Path.Combine(BasePath, "video");
    private static readonly string LogDir = Path.Combine(BasePath, "logs");

    private static readonly string LogFile =
        Path.Combine(LogDir, $"{Assembly.GetEntryAssembly()!.GetName().Name}.log");

    private static async Task PressKeyWithDelay(KeyCode keyCode, int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            InputSimulator.PressKey(keyCode);
            await Task.Delay(50);
        }
    }

    private static async Task Main(string[] args)
    {
        Directory.CreateDirectory(LogDir);
        Directory.CreateDirectory(DownloadedDir);
        Directory.CreateDirectory(VideoDir);

        SelfLog.Enable(Console.Error);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(LogFile, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var storagePrefix = UserFileNamePrefixGenerator.Generate();
        var runTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var storageRoot = $"{storagePrefix}/{runTimestamp}";

        string? saJson = null;
        string? bucketName = null;
        string? ceLogin;
        string? cePassword;

        ScreenRecorder? recorder = null;
        string? videoPath = null;
        string? reportFilePath = null;
        var isSuccess = false;
        var recordingStopped = false;

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Log.Warning("Ctrl+C detected: stopping recording.");
            if (recorder != null && !recordingStopped)
                try
                {
                    _ = recorder.StopAsync();
                    recordingStopped = true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while stopping recorder on Ctrl+C.");
                }
        };

        try
        {
            var current = Process.GetCurrentProcess();

            var duplicates = Process.GetProcessesByName(current.ProcessName)
                .Where(p =>
                {
                    if (p.Id == current.Id) return false;

                    try
                    {
                        return p.MainModule?.FileName == current.MainModule?.FileName;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToList();

            foreach (var dup in duplicates)
            {
                Log.Warning("Another instance detected (PID {Pid}). Attempting to terminate it.", dup.Id);
                try
                {
                    dup.Kill(true);
                    dup.WaitForExit(5000);
                    Log.Information("Terminated duplicate instance PID {Pid}.", dup.Id);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to terminate duplicate instance PID {Pid}.", dup.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed while checking for duplicate instances.");
        }

        try
        {
            Log.Information("Initializing HCP client.");
            using var httpClient = new HttpClient();
            var hcpClient = new HcpClient(httpClient);

            Log.Information("Requesting HCP access token.");
            var token = await hcpClient.GetAccessTokenAsync()
                        ?? throw new Exception("Failed to retrieve HCP access token.");

            saJson = await hcpClient.FetchSecretAsync(token, AppConfig.FirebaseSecretName)
                     ?? throw new Exception("Failed to fetch the 'firebase' secret from HCP.");

            bucketName = await hcpClient.FetchSecretAsync(token, AppConfig.BucketName)
                         ?? throw new Exception("Failed to fetch the 'bucket_name' secret from HCP.");

            ceLogin = await hcpClient.FetchSecretAsync(token, AppConfig.CeLogin)
                      ?? throw new Exception("Failed to fetch the 'ce_login' secret from HCP.");

            cePassword = await hcpClient.FetchSecretAsync(token, AppConfig.CePassword)
                         ?? throw new Exception("Failed to fetch the 'ce_password' secret from HCP.");

            recorder = new ScreenRecorder();
            var videoName = Path.ChangeExtension(VideoNameGenerator.Generate(), ".mkv");
            videoPath = Path.Combine(VideoDir, videoName);

            Log.Information("Starting screen recording.");
            await recorder.StartAsync(videoPath, 10);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log.Information("Counting files with headless Microsoft Edge.");
                var edgeOneOptions = new EdgeOptions();
                edgeOneOptions.AddArgument("--headless=new");
                edgeOneOptions.AddArgument("--edge-skip-compat-layer-relaunch");
                using var edgeCounter = new EdgeDriver(edgeOneOptions);
                BrowserLogic.WebsiteLogin(edgeCounter, ceLogin!, cePassword!);
                BrowserLogic.LinksCount = BrowserLogic.CountAllFiles(edgeCounter);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Log.Information("Counting files Apple Safari.");
                using var safariCounter = new SafariDriver();
                BrowserLogic.WebsiteLogin(safariCounter, ceLogin!, cePassword!);
                BrowserLogic.LinksCount = BrowserLogic.CountAllFiles(safariCounter);
            }


            Log.Information("Checking if Google Chrome is installed.");
            if (BrowserDetector.IsInstalled(BrowserType.Chrome))
            {
                Log.Information("Starting Google Chrome automation.");
                var chromeOptions = (ChromeOptions)BrowserConfiguratorFactory
                    .GetConfigurator(BrowserType.Chrome)
                    .Configure(DownloadedDir);

                using (var chromeDriver = new ChromeDriver(chromeOptions))
                {
                    chromeDriver.Manage().Window.Maximize();
                    BrowserLogic.DownloadWorkflow(chromeDriver, ceLogin!, cePassword!);
                    chromeDriver.Navigate().GoToUrl("chrome://downloads");
                    await Task.Delay(7000);
                    await PressKeyWithDelay(KeyCode.VcTab);
                    await PressKeyWithDelay(KeyCode.VcEnter);
                    await PressKeyWithDelay(KeyCode.VcTab, 6);
                    for (var i = 0; i < BrowserLogic.LinksCount * 3; i++)
                    {
                        await PressKeyWithDelay(KeyCode.VcDown);
                        await Task.Delay(180);
                    }
                }

                FileTools.CleanDirectory(DownloadedDir);
            }

            Log.Information("Checking if Microsoft Edge is installed.");
            if (BrowserDetector.IsInstalled(BrowserType.Edge))
            {
                Log.Information("Starting Microsoft Edge automation.");
                var edgeOptions = (EdgeOptions)BrowserConfiguratorFactory
                    .GetConfigurator(BrowserType.Edge)
                    .Configure(DownloadedDir);

                using (var edgeDriver = new EdgeDriver(edgeOptions))
                {
                    edgeDriver.Navigate().GoToUrl("edge://settings/downloads");
                    await Task.Delay(2000);
                    var wait = new WebDriverWait(edgeDriver, TimeSpan.FromSeconds(10));
                    var checkboxes = wait.Until(driver =>
                    {
                        var elements = driver.FindElements(By.XPath("//input[@type='checkbox']"));
                        return elements.Count >= 2 ? elements : null;
                    });

                    var count = checkboxes.Count;
                    checkboxes[count - 2].Click();
                    checkboxes[count - 1].Click();

                    BrowserLogic.DownloadWorkflow(edgeDriver, ceLogin!, cePassword!);
                    edgeDriver.Navigate().GoToUrl("edge://downloads");

                    var screens = UioHook.CreateScreenInfo();
                    var maxRight = screens.Max(s => s.X + s.Width);
                    var maxBottom = screens.Max(s => s.Y + s.Height);

                    const int cornerOffset = 150;
                    var clickX = (short)(maxRight - cornerOffset);
                    var clickY = (short)(maxBottom - cornerOffset);

                    var simulator = new EventSimulator();
                    simulator.SimulateMouseMovement(clickX, clickY);
                    await Task.Delay(50);
                    simulator.SimulateMousePress(MouseButton.Button1);
                    simulator.SimulateMouseRelease(MouseButton.Button1);
                    await Task.Delay(600);
                    for (var b = 1; b < BrowserLogic.LinksCount * 3; b++)
                    {
                        await PressKeyWithDelay(KeyCode.VcDown);
                        await Task.Delay(180);
                    }
                }

                FileTools.CleanDirectory(DownloadedDir);
            }


            Log.Information("Checking if Mozilla Firefox is installed.");
            if (BrowserDetector.IsInstalled(BrowserType.Firefox))
            {
                Log.Information("Starting Mozilla Firefox automation.");
                var firefoxOptions = (FirefoxOptions)BrowserConfiguratorFactory
                    .GetConfigurator(BrowserType.Firefox)
                    .Configure(DownloadedDir);

                using (var ffDriver = new FirefoxDriver(firefoxOptions))
                {
                    ffDriver.Manage().Window.Maximize();
                    BrowserLogic.DownloadWorkflow(ffDriver, ceLogin!, cePassword!);
                    ffDriver.Navigate().GoToUrl("about:downloads");
                    await PressKeyWithDelay(KeyCode.VcTab, 9);
                    for (var i = 0; i < BrowserLogic.LinksCount; i++)
                    {
                        await PressKeyWithDelay(KeyCode.VcDown);
                        await Task.Delay(180);
                    }
                }

                FileTools.CleanDirectory(DownloadedDir);
            }


            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Log.Information("Starting Safari automation.");
                using (var safari = new SafariDriver())
                {
                    safari.Manage().Window.Maximize();
                    BrowserLogic.DownloadWorkflow(safari, ceLogin!, cePassword!);
                }

                Process.Start("open",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Downloads");
                await Task.Delay(2000);
                InputSimulator.Hotkey(KeyCode.VcLeftControl, KeyCode.VcLeftMeta, KeyCode.VcF);
                await Task.Delay(2000);
            }

            Log.Information("ping screen recording.");
            await recorder.StopAsync(TimeSpan.FromSeconds(10));
            recordingStopped = true;

            var uploader = new FirebaseUploader(saJson, bucketName);
            var remoteVideoPath = StoragePathGenerator.GetRemotePath(storageRoot, videoPath);
            Log.Information("Uploading video as {RemotePath}", remoteVideoPath);
            await uploader.UploadFileAsync(videoPath, remoteVideoPath);

            reportFilePath = await new SystemReportService().GenerateAsync();
            var remoteReportPath = StoragePathGenerator.GetRemotePath(storageRoot, reportFilePath);
            Log.Information("Uploading report as {RemotePath}", remoteReportPath);
            await uploader.UploadFileAsync(reportFilePath, remoteReportPath);

            isSuccess = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred");
        }
        finally
        {
            try
            {
                if (!recordingStopped)
                    recorder?.StopAsync();
            }
            catch (Exception stopEx)
            {
                Log.Error(stopEx, "Failed to stop screen recording in finally block.");
            }

            if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
                try
                {
                    File.Delete(videoPath);
                }
                catch (Exception deleteEx)
                {
                    Log.Error(deleteEx, "Failed to delete incomplete video file.");
                }

            try
            {
                if (Directory.Exists(DownloadedDir)) Directory.Delete(DownloadedDir, true);
                if (Directory.Exists(VideoDir)) Directory.Delete(VideoDir, true);
                if (!string.IsNullOrEmpty(reportFilePath) && File.Exists(reportFilePath))
                    File.Delete(reportFilePath);
            }
            catch (Exception cleanupEx)
            {
                Log.Error(cleanupEx, "Failed to clean up working directories.");
            }

            await Log.CloseAndFlushAsync();

            var endTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            try
            {
                var logUploader = new FirebaseUploader(saJson!, bucketName!);

                var logDirPath = Path.GetDirectoryName(LogFile)!;
                var logBaseName = Path.GetFileNameWithoutExtension(LogFile);
                var candidates = Directory.GetFiles(logDirPath, $"{logBaseName}*.log");

                var latest = candidates
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(fi => fi.LastWriteTimeUtc)
                    .FirstOrDefault()?.FullName;

                if (latest != null)
                {
                    var remoteLogPath = $"{storageRoot}/logs/{storagePrefix}_{endTimestamp}.logs";
                    Console.WriteLine($"Uploading logs as {remoteLogPath}");
                    await logUploader.UploadFileAsync(latest, remoteLogPath);
                }
                else
                {
                    await Console.Error.WriteLineAsync("No log files found to upload.");
                }
            }
            catch (Exception uploadLogEx)
            {
                await Console.Error.WriteLineAsync($"Failed to upload log file: {uploadLogEx}");
            }

            if (isSuccess)
                try
                {
                    if (Directory.Exists(LogDir)) Directory.Delete(LogDir, true);
                    SuccessNotifier.Show();
                }
                catch (Exception logDelEx)
                {
                    await Console.Error.WriteLineAsync($"Failed to delete log directory: {logDelEx.Message}");
                }

            SelfDeleter.SelfDelete();
        }
    }
}