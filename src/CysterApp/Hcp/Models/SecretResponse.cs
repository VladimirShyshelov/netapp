using System.Text.Json.Serialization;

namespace CysterApp.Hcp.Models;

internal class SecretResponse
{
    [JsonPropertyName("secret")] public SecretDetail Secret { get; init; } = null!;
}