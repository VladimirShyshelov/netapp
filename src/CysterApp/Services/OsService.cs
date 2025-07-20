using System.Globalization;
using System.Management;
using CysterApp.Models;

namespace CysterApp.Services;

public class OsService
{
    private const string Csv = """
                               OS,Version,Build,HomePro,EnterpriseEdu
                               Windows 11,24H2,26100,2026/10/13,2027/10/12
                               Windows 11,23H2,22631,2025/11/11,2026/11/10
                               Windows 11,22H2,22621,2024/10/08,2025/10/14
                               Windows 11,21H2,22000,2023/10/10,2024/10/08
                               Windows 10,22H2,19045,2025/10/14,2025/10/14
                               Windows 10,21H2,19044,2023/06/13,2024/06/11
                               Windows 10,21H1,19043,2022/12/13,2022/12/13
                               Windows 10,20H2,19042,2022/05/10,2023/05/09
                               Windows 10,2004,19041,2021/12/14,2021/12/14
                               Windows 10,1909,18363,2021/05/11,2022/05/10
                               Windows 10,1903,18362,2020/12/08,2020/12/08
                               Windows 10,1809,17763,2020/11/10,2021/05/11
                               Windows 10,1803,17134,2019/11/12,2021/05/11
                               Windows 10,1709,16299,2019/04/09,2020/10/13
                               Windows 10,1703,15063,2018/10/09,2019/10/08
                               Windows 10,1607,14393,2018/04/10,2019/04/09
                               Windows 10,1511,10586,2017/10/10,2017/10/10
                               Windows 10,1507,10240,2017/05/09,2017/05/09
                               """;

    public (bool IsEol, string Message) GetEol()
    {
        var row = Parse().FirstOrDefault(r => r.Build == BuildNumber());
        var sku = SkuName();

        if (row is null)
            return (false, $"No EOL data for build {BuildNumber()}");

        var limit = sku is "Enterprise" or "Education" ? row.EnterpriseEdu : row.HomePro;
        var date = limit.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);

        return DateTime.Now > limit
            ? (true, $"Support for {row.Os} Version {row.Version} ({sku}) ended on {date}")
            : (false,
                $"Support for {row.Os} Version {row.Version} ({sku}) ends in {(limit - DateTime.Now).Days} " +
                $"days on {date}");
    }

    private static IEnumerable<EolInfo> Parse()
    {
        return Csv.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(line =>
                line.Trim().Split(','))
            .Select(p => new EolInfo(
                p[0], p[1], int.Parse(p[2]),
                DateTime.Parse(p[3], CultureInfo.InvariantCulture),
                DateTime.Parse(p[4], CultureInfo.InvariantCulture)));
    }

    private static int BuildNumber()
    {
        return int.Parse(Query("BuildNumber"));
    }

    private static string SkuName()
    {
        var caption = Query("Caption");
        return caption.Contains("Ent", StringComparison.OrdinalIgnoreCase) ? "Enterprise"
            : caption.Contains("Edu", StringComparison.OrdinalIgnoreCase) ? "Education"
            : caption.Contains("Pro", StringComparison.OrdinalIgnoreCase) ? "Pro/Home"
            : "Unknown";
    }

    private static string Query(string prop)
    {
        var m = new ManagementObjectSearcher($"SELECT {prop} FROM Win32_OperatingSystem")
            .Get().Cast<ManagementObject>().First();
        return m[prop]!.ToString()!;
    }
}