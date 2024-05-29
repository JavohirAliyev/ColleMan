using ColleMan.Interfaces;
using ColleMan.Migrations;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace ColleMan.Models
{
    public class GoogleCloud : ICloud
    {
        private readonly GoogleCredential googleCredential;
        private readonly StorageClient storageClient;
        private readonly string bucket;
        public GoogleCloud(IConfiguration configuration)
        {
            googleCredential = GoogleCredential.FromFile(configuration.GetValue<string>("GoogleCredentialFile"));
            storageClient = StorageClient.Create(googleCredential);
            bucket = configuration.GetValue<string>("GoogleCloudStorageBucket");
        }
        public async Task DeleteImageAsync(string imageName)
        {
            await storageClient.DeleteObjectAsync(bucket, imageName);
        }

        public async Task<string> UploadImageAsync(IFormFile image, string imageName)
        {
            using (var mem = new MemoryStream())
            {
                await image.CopyToAsync(mem);
                var dataObject = await storageClient.UploadObjectAsync(bucket, imageName, null, mem);
                return dataObject.MediaLink;
            }
        }
    }
}
