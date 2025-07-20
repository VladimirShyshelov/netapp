using System.Runtime.InteropServices;

namespace CysterApp.Services;

internal static class UserFileNamePrefixGenerator
{
    public static string Generate()
    {
        string prefix;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var domain = Environment.GetEnvironmentVariable("USERDOMAIN") ?? "UNKNOWN_DOMAIN";
            var username = Environment.UserName;
            prefix = $"{domain}_{username}";
        }
        else
        {
            var machineName = Environment.MachineName;
            var username = Environment.UserName;
            prefix = $"{machineName}_{username}";
        }

        return prefix;
    }
}