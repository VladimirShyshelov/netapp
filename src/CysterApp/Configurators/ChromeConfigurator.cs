using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CysterApp.Configurators;

internal class ChromeConfigurator : IBrowserConfigurator
{
    public DriverOptions Configure(string downloadDir)
    {
        var opts = new ChromeOptions();
        opts.AddUserProfilePreference("download.default_directory", downloadDir);
        opts.AddUserProfilePreference("download.prompt_for_download", false);
        opts.AddUserProfilePreference("profile.default_content_settings.popups", 0);
        opts.AddUserProfilePreference("safebrowsing.enabled", true);
        opts.AddUserProfilePreference("download.directory_upgrade", true);
        opts.AddUserProfilePreference("profile.content_settings.exceptions.automatic_downloads.*.setting", 1);
        opts.AddArgument("--lang=en-US");
        opts.AddArgument("--start-maximized");
        opts.AddArgument("--disable-gpu");
        return opts;
    }
}