using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;

namespace CysterApp.WebBrowser;

internal static class BrowserLogic
{
    private const int DefaultWaitTime = 60;

    private static readonly string[] Urls = new[]
    {
        "https://oyster-app-xnc8n.ondigitalocean.app/amtso_files/",
        "https://oyster-app-xnc8n.ondigitalocean.app/browser_test/"
    };

    public static int LinksCount { get; set; }


    private static IWebElement WaitForElement(IWebDriver driver, string selector,
        int timeoutSeconds = DefaultWaitTime)
    {
        try
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(ExpectedConditions.ElementExists(By.CssSelector(selector)));
        }
        catch (WebDriverTimeoutException ex)
        {
            Log.Error(ex, "Element with selector '{Selector}' not found within {Timeout} seconds.", selector,
                timeoutSeconds);
            return null;
        }
    }

    private static IReadOnlyCollection<IWebElement> WaitAndFindElements(IWebDriver driver, string selector,
        int timeoutSeconds = DefaultWaitTime)
    {
        try
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(ExpectedConditions.ElementExists(By.CssSelector(selector)));
            return driver.FindElements(By.CssSelector(selector));
        }
        catch (WebDriverTimeoutException)
        {
            Log.Warning("No elements found with selector '{Selector}' within {Timeout} seconds.", selector,
                timeoutSeconds);
            return Array.Empty<IWebElement>();
        }
    }

    public static void WebsiteLogin(IWebDriver driver, string login, string password)
    {
        const string loginUrl = "https://oyster-app-xnc8n.ondigitalocean.app/login/";
        driver.Navigate().GoToUrl(loginUrl);
        Log.Information("Opened login page: {LoginUrl}", loginUrl);

        var usernameElement = WaitForElement(driver, "#id_username");
        var passwordElement = WaitForElement(driver, "#id_password");
        var loginButton = WaitForElement(driver, "body > div > form > div.flex.justify-center.mt-4 > button");

        if (usernameElement != null && passwordElement != null && loginButton != null)
        {
            usernameElement.SendKeys(login);
            passwordElement.SendKeys(password);
            loginButton.Click();
            Thread.Sleep(1000);
            Log.Information("Submitted login for user '{Login}'.", login);
        }
        else
        {
            Log.Error("Failed to find one or more login elements on {LoginUrl}.", loginUrl);
            throw new NoSuchElementException("Login page elements not found.");
        }
    }

    public static int CountAllFiles(IWebDriver driver)
    {
        var totalFiles = 0;

        foreach (var url in Urls)
        {
            Log.Information("Navigating to {Url} to count files.", url);
            driver.Navigate().GoToUrl(url);

            var links = WaitAndFindElements(driver, "a[href*='.']");
            var count = links.Count;
            Log.Information("Found {Count} file link(s) on {Url}.", count, url);

            totalFiles += count;
        }

        LinksCount = totalFiles;
        Log.Information("Total files across all URLs: {TotalFiles}.", totalFiles);
        return totalFiles;
    }

    public static void DownloadAllFilesFromUrl(IWebDriver driver)
    {
        Log.Information("Starting file download process.");

        foreach (var url in Urls)
        {
            Log.Information("Navigating to {Url} to download files.", url);
            driver.Navigate().GoToUrl(url);

            var links = WaitAndFindElements(driver, "a[href*='.']");
            if (links.Any())
            {
                Log.Information("Found {Count} file link(s) on {Url}. Beginning downloads.", links.Count, url);
                var index = 1;

                foreach (var link in links)
                {
                    var href = link.GetAttribute("href");
                    var fileName = href?.Split('/').Last() ?? "unknown";
                    Log.Information("Downloading file {Index}: {FileName}", index, fileName);

                    try
                    {
                        link.Click();
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to click link for file '{FileName}' at {Href}.", fileName, href);
                    }

                    index++;
                }
            }
            else
            {
                Log.Warning("No downloadable files found on {Url}.", url);
            }
        }

        Log.Information("File download process completed.");
    }

    public static void DownloadWorkflow(IWebDriver driver, string login, string password)
    {
        try
        {
            WebsiteLogin(driver, login, password);
            DownloadAllFilesFromUrl(driver);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Download workflow failed.");
            throw;
        }
    }
}