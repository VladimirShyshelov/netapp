namespace CysterApp.Services;

internal static class StoragePathGenerator
{
    public static string GetRemotePath(string userPrefix, string localFilePath)
    {
        var extension = Path.GetExtension(localFilePath)?.ToLowerInvariant();
        var folder = extension switch
        {
            ".mkv" => "video",
            ".html" => "html",
            ".htm" => "html",
            _ => "files"
        };

        var fileName = Path.GetFileName(localFilePath);
        return $"{userPrefix}/{folder}/{fileName}";
    }
}