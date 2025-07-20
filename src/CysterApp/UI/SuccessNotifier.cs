using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace CysterApp.UI;

public static class SuccessNotifier
{
    public static void Show()
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime([]);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<SuccessApp>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}

internal sealed class SuccessApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            var ok = new Button
            {
                Content = "OK",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            ok.Click += (_, __) => lifetime.Shutdown();

            lifetime.MainWindow = new Window
            {
                Title = "Done",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,

                Content = new StackPanel
                {
                    Margin = new Thickness(40),
                    Spacing = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text =
                                "All tests have completed successfully and all files have been deleted.\n" +
                                "You may delete this text file and continue with your day.",
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextWrapping = TextWrapping.Wrap
                        },
                        ok
                    }
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}