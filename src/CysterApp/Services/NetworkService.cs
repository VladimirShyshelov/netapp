using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CysterApp.Services;

public class NetworkService
{
    public static string GetLocalIp()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up &&
                        i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(i => i.GetIPProperties().UnicastAddresses)
            .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork &&
                                 !IPAddress.IsLoopback(a.Address))
            ?.Address.ToString() ?? "N/A";
    }

    public static string GetSubnetMask()
    {
        var maskInfo = NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up &&
                        i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(i => i.GetIPProperties().UnicastAddresses)
            .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork &&
                                 !IPAddress.IsLoopback(a.Address));
        return maskInfo?.IPv4Mask.ToString() ?? "N/A";
    }

    public static string GetGateway()
    {
        var gatewayInfo = NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up &&
                        i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(i => i.GetIPProperties().GatewayAddresses)
            .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork);
        return gatewayInfo?.Address.ToString() ?? "N/A";
    }

    public static async Task<string> GetPublicIpAsync()
    {
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(5);
        try
        {
            return (await http.GetStringAsync("http://ifconfig.me/ip")).Trim();
        }
        catch
        {
            return "Unavailable";
        }
    }
}