using System.Net.Http.Headers;
using System.Text.Json;
using CysterApp.Config;
using CysterApp.Hcp.Models;

namespace CysterApp.Hcp.Clients;

public class HcpClient(HttpClient httpClient) : IHcpClient
{
    public async Task<string?> GetAccessTokenAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, AppConfig.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", AppConfig.ClientId),
                new KeyValuePair<string, string>("client_secret", AppConfig.ClientSecret),
                new KeyValuePair<string, string>("audience", AppConfig.Audience)
            ])
        };

        var res = await httpClient.SendAsync(req);
        if (!res.IsSuccessStatusCode) return null;

        var json = await res.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<TokenResponse>(json);
        return dto?.AccessToken;
    }

    public async Task<string?> FetchSecretAsync(string token, string secretName)
    {
        var url = $"{AppConfig.ApiBaseUrl}"
                  + $"/organizations/{AppConfig.OrgId}"
                  + $"/projects/{AppConfig.ProjectId}"
                  + $"/apps/{AppConfig.AppName}"
                  + $"/secrets/{secretName}:open";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await httpClient.SendAsync(req);
        if (!res.IsSuccessStatusCode) return null;

        var json = await res.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<SecretResponse>(json);
        return dto?.Secret.StaticVersion.Value;
    }
}