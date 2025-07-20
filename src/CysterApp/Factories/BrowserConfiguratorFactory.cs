using CysterApp.Configurators;

namespace CysterApp.Factories;

public enum BrowserType
{
    Chrome,
    Edge,
    Firefox
}

internal static class BrowserConfiguratorFactory
{
    public static IBrowserConfigurator GetConfigurator(BrowserType type)
    {
        switch (type)
        {
            case BrowserType.Chrome:
                return new ChromeConfigurator();
            case BrowserType.Edge:
                return new EdgeConfigurator();
            case BrowserType.Firefox:
                return new FirefoxConfigurator();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}