using Academy.Core.Models;
using Azure.Storage.Blobs;
using Azure.Identity;
using Academy.Core.Abstraction.Infrastructure;
using Azure.Storage.Blobs.Models;
namespace Academy.Infrastructure.AzureStorageClient
{
    public class Blob : IBlob
    {
        private readonly BlobServiceClient blobServiceClient;
        public Blob(IAzureStorageSettings azureStorageSettings)
        {
            blobServiceClient = new(new Uri(azureStorageSettings?.ConnectionString), new DefaultAzureCredential());
        }
        public async Task<string> CreateBlobIfNotExistsAsync(string containerName)
        {
            BlobContainerClient containerClient  = await CreateAsync(containerName);
            Uri containerUri = containerClient.Uri;
            return containerUri.ToString();
        }
        public async Task<string> UploadAsync(string containerName, string fileName, Stream fileContent)
        {
            BlobContainerClient containerClient = await CreateAsync(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            var result = await blobClient.UploadAsync(fileContent, true);
            return blobClient.Uri.ToString();
        }
        public async Task<bool> DeleteIfExistsAsync(string containerName, string fileName)
        {
            BlobContainerClient containerClient = await CreateAsync(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None);
            return true;
        }
        public async Task<Stream> DownloadAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            var stream = await blobClient.OpenReadAsync();
            return stream;
        }

        private async Task<BlobContainerClient> CreateAsync(string containerName)
        {
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }
    }
}
