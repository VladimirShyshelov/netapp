using System.Text.Json.Serialization;

namespace CysterApp.Hcp.Models;

internal class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = null!;
}