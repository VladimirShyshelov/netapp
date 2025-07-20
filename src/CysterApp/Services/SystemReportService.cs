using System.Runtime.InteropServices;
using CysterApp.Models;
using CysterApp.Utilities;

namespace CysterApp.Services;

public class SystemReportService
{
    private readonly NetworkService _net = new();
    private readonly OsService _os = new();
    private readonly UserPrivilegeService _priv = new();
    private readonly SecurityService _sec = new();

    public async Task<string> GenerateAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null!;

        var rows = new List<Result>
        {
            new(true, "Get Username", Environment.UserName),
            new(true, "Get ComputerName", Environment.MachineName),
            new(true, "Get IPAddress", NetworkService.GetLocalIp()),
            new(true, "Get Public IPAddress", await NetworkService.GetPublicIpAsync())
        };

        rows.Add(new Result(true, "Get Subnet Mask", NetworkService.GetSubnetMask()));
        rows.Add(new Result(true, "Get Default Gateway", NetworkService.GetGateway()));

        var (isEol, eolMsg) = _os.GetEol();
        rows.Add(new Result(!isEol, "Supported OS", eolMsg));

        var isAdmin = _priv.IsAdmin();
        rows.Add(new Result(!isAdmin, "Is Admin", isAdmin.ToString()));

        var avOk = _sec.IsAvOk();
        rows.Add(new Result(avOk, "Get AV Status", _sec.AvHuman()));

        var fwOn = _sec.IsFirewallOn();
        rows.Add(new Result(fwOn, "Get Firewall Status", _sec.FirewallHuman()));

        var html = HtmlReportBuilder.Build(rows, _priv.GetRolesText());
        var programDirectory = AppContext.BaseDirectory;
        var domain = Environment.GetEnvironmentVariable("USERDOMAIN") ?? "UNKNOWN_DOMAIN";
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var report = Path.Combine(programDirectory,
            $"{domain}_{Environment.UserName}_{timestamp}.html");

        await File.WriteAllTextAsync(report, html);
        return report;
    }
}