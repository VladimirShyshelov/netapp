namespace CysterApp.Models;

public class AntivirusInfo
{
    public string DisplayName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool UpToDate { get; set; }
}