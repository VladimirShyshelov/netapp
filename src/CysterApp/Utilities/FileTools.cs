namespace CysterApp.Utilities;

internal static class FileTools
{
    public static void CleanDirectory(string directory)
    {
        if (!Directory.Exists(directory)) return;
        foreach (var file in Directory.GetFiles(directory))
            File.Delete(file);
    }
}