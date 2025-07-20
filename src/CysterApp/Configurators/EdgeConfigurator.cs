using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace CysterApp.Configurators;

internal class EdgeConfigurator : IBrowserConfigurator
{
    public DriverOptions Configure(string downloadDir)
    {
        var opts = new EdgeOptions();
        opts.AddUserProfilePreference("download.default_directory", downloadDir);
        opts.AddUserProfilePreference("download.prompt_for_download", false);
        opts.AddUserProfilePreference("profile.default_content_settings.popups", 0);
        opts.AddUserProfilePreference("safebrowsing.enabled", true);
        opts.AddUserProfilePreference("download.directory_upgrade", true);
        opts.AddUserProfilePreference("profile.content_settings.exceptions.automatic_downloads.*.setting", 1);
        opts.AddUserProfilePreference("savefile.default_directory", downloadDir);
        opts.AddUserProfilePreference("download.open_pdf_in_system_reader", false);
        opts.AddArgument("--start-maximized");
        opts.AddArgument("--lang=en-US");
        opts.AddArgument("--disable-gpu");

        opts.AddArgument("--edge-skip-compat-layer-relaunch");

        var uuid = Guid.NewGuid().ToString();
        var userDataDir = Path.Combine(Path.GetTempPath(), $"edge-{uuid}");
        Directory.CreateDirectory(userDataDir);
        opts.AddArgument($"--user-data-dir={userDataDir}");
        return opts;
    }
}