using OpenQA.Selenium;
using OpenQA.Selenium.Safari;

namespace CysterApp.Configurators;

internal class SafariConfigurator : IBrowserConfigurator
{
    public DriverOptions Configure(string downloadDir)
    {
        var opts = new SafariOptions();
        return opts;
    }
}