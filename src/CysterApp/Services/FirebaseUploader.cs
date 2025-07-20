using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace CysterApp.Services;

public class FirebaseUploader(string serviceAccountJson, string bucketName)
{
    public async Task UploadFileAsync(string localPath, string remoteName)
    {
        var credential = GoogleCredential.FromJson(serviceAccountJson);
        var storageClient = await StorageClient.CreateAsync(credential);

        await using var fileStream = File.OpenRead(localPath);
        await storageClient.UploadObjectAsync(
            bucketName,
            remoteName,
            null,
            fileStream
        );
    }
}