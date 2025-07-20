namespace CysterApp.Services;

internal static class VideoNameGenerator
{
    public static string Generate()
    {
        var prefix = UserFileNamePrefixGenerator.Generate();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return $"{prefix}_{timestamp}";
    }
}