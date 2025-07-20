using System.Text.Json.Serialization;

namespace CysterApp.Hcp.Models;

internal class SecretDetail
{
    [JsonPropertyName("static_version")] public StaticVersion StaticVersion { get; set; } = null!;
}