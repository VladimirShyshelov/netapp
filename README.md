CysterApp

# CysterApp

## Overview

CysterApp is a .NET console application that automates the following workflow end-to-end:

1. **Fetch secrets** (Firebase credentials, storage bucket name, CE login/password) from an HCP (HashiCorp) secrets
   store.
2. **Count** and **download** files from a CE web portal using Selenium WebDriver (headless Chrome, Chrome, Edge,
   Firefox, and Safari on macOS).
3. **Record** the entire browser session to a video (`.mkv`) using the built-in `ScreenRecorder`.
4. **Upload** the recording and a generated system report to Firebase Storage.
5. **Log** all operations via Serilog.
6. **Clean up** all temporary files and directories.
7. **Self-delete** the executable if it wasn’t launched as a `.dll` (optional).

## Features

- **HCP integration** via `HcpClient` to retrieve secrets securely
- **Multi-browser support** with Selenium WebDriver (Chrome, Edge, Firefox, Safari)
- **Headless-mode counting** of available download links before full automation
- **Screen recording** with adjustable duration segments
- **Automatic upload** of `.mkv` video and system reports to Firebase
- **Robust error handling**: logs exceptions, stops recorder, cleans up half-finished files
- **Self-cleanup**: deletes downloaded files, videos, reports, logs after a successful run

## Prerequisites

- [.NET 9.0+ SDK/Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0/)
- Browser WebDriver binaries on `PATH` (matching installed browsers):
    - **ChromeDriver**
    - **EdgeDriver**
    - **GeckoDriver** (Firefox)
    - **SafariDriver** (macOS)
- Access to your HCP vault with secrets named in `AppConfig`
- A Firebase project with Storage enabled

## Configuration

Update the secret-names in `CysterApp.Config\AppConfig.cs`:

```csharp
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
```

## Compile (Windows)

```powershell
dotnet publish CysterApp.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:PublishTrimmed=false /p:PublishReadyToRun=false
```
