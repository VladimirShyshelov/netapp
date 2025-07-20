using System.Management;
using System.Runtime.InteropServices;
using CysterApp.Models;

namespace CysterApp.Services;

public class SecurityService
{
    public IReadOnlyList<AntivirusInfo> GetAntivirus()
    {
        var list = new List<AntivirusInfo>();
        var q = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntivirusProduct");

        foreach (ManagementObject m in q.Get())
        {
            var hex = ((uint)m["productState"]).ToString("X6");
            var enabled = hex.Substring(2, 2) is not ("00" or "01");
            var upToDate = hex.Substring(4, 2) == "00";

            list.Add(new AntivirusInfo
            {
                DisplayName = m["displayName"]?.ToString() ?? "Unknown AV",
                Enabled = enabled,
                UpToDate = upToDate
            });
        }

        return list;
    }

    public bool IsAvOk()
    {
        return GetAntivirus().Any(a => a.Enabled && a.UpToDate);
    }

    public string AvHuman()
    {
        return string.Join("<br/>", GetAntivirus()
            .Select(a => $"{a.DisplayName}, {(a.Enabled ? "Enabled" : "Disabled")}, " +
                         $"{(a.UpToDate ? "UpToDate" : "OutOfDate")}"));
    }

    public IReadOnlyList<FirewallInfo> GetFirewalls()
    {
        var list = new List<FirewallInfo>();

        try
        {
            var progId = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")
                         ?? throw new COMException("Firewall API not registered");
            dynamic policy = Activator.CreateInstance(progId)!;

            (Profile Id, string Name)[] profiles =
            [
                (Profile.Domain, "Windows Firewall Domain Profile"),
                (Profile.Private, "Windows Firewall Private Profile"),
                (Profile.Public, "Windows Firewall Public Profile")
            ];

            foreach (var (id, name) in profiles)
            {
                bool enabled = policy.FirewallEnabled[(int)id];
                list.Add(new FirewallInfo
                {
                    ProductName = name,
                    ProductState = enabled ? "Enabled" : "Disabled"
                });
            }
        }
        catch (Exception ex)
        {
            list.Add(new FirewallInfo
            {
                ProductName = "Windows Firewall",
                ProductState = $"Unknown ({ex.Message})"
            });
        }

        return list;
    }

    public bool IsFirewallOn()
    {
        return GetFirewalls().Any(f => f.ProductState == "Enabled");
    }

    public string FirewallHuman()
    {
        return string.Join("<br/>", GetFirewalls()
            .Select(f => $"{f.ProductName}, {f.ProductState}"));
    }

    private enum Profile
    {
        Domain = 1,
        Private = 2,
        Public = 4
    }
}