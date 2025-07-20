using OpenQA.Selenium;

namespace CysterApp.Configurators;

internal interface IBrowserConfigurator
{
    DriverOptions Configure(string downloadDir);
}