using System.Text.Json.Serialization;

namespace CysterApp.Hcp.Models;

internal class StaticVersion
{
    [JsonPropertyName("value")] public string Value { get; set; } = null!;
}