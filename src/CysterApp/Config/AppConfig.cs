namespace CysterApp.Config;

public class AppConfig
{
    public const string ClientId = "ZzYSHYxHcgSVO01JcnZCylaAWjZlAsrv";
    public const string ClientSecret = "3jGWKFOs-4PwxI35Q-VIryFHua1n9YniEhaEPqMAyn8K0Nhbg6MOe2t-ZnUXyuph";
    public const string TokenEndpoint = "https://auth.idp.hashicorp.com/oauth2/token";
    public const string Audience = "https://api.hashicorp.cloud";
    public const string ApiBaseUrl = "https://api.cloud.hashicorp.com/secrets/2023-11-28";
    public const string OrgId = "a5c1b37c-71b3-483c-8dec-5aed7bc82659";
    public const string ProjectId = "6f50f463-9511-44dd-8d10-0125bfc06b1e";
    public const string AppName = "sample-app";

    // vault
    public const string FirebaseSecretName = "firebase";
    public const string BucketName = "bucket_name";
    public const string CeLogin = "ce_login";
    public const string CePassword = "ce_password";
}