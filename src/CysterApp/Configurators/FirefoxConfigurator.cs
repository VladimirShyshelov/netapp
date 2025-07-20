using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace CysterApp.Configurators;

internal class FirefoxConfigurator : IBrowserConfigurator
{
    public DriverOptions Configure(string downloadDir)
    {
        var opts = new FirefoxOptions();
        opts.SetPreference("browser.download.folderList", 2);
        opts.SetPreference("browser.download.dir", downloadDir);
        opts.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/pdf,application/octet-stream");
        opts.SetPreference("pdfjs.disabled", true);
        opts.SetPreference("browser.download.manager.showWhenStarting", false);
        opts.SetPreference("browser.download.animateNotifications", false);
        opts.SetPreference("browser.download.manager.closeWhenDone", true);
        opts.SetPreference("browser.download.manager.showAlertOnComplete", false);
        opts.SetPreference("browser.download.manager.focusWhenStarting", false);
        opts.SetPreference("intl.accept_languages", "en-US");
        opts.SetPreference("layers.acceleration.disabled", true);
        opts.SetPreference("gfx.webrender.software", true);
        opts.SetPreference("gfx.webrender.enabled", true);
        opts.SetPreference("media.ffmpeg.vaapi.enabled", false);
        opts.AddArgument("--start-maximized");

        return opts;
    }
}