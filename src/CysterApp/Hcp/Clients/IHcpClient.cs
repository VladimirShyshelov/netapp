namespace CysterApp.Hcp.Clients;

public interface IHcpClient
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> FetchSecretAsync(string token, string secretName);
}