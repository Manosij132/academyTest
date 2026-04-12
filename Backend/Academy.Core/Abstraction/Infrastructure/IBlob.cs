namespace Academy.Core.Abstraction.Infrastructure
{
    public interface IBlob
    {
        Task<string> CreateBlobIfNotExistsAsync(string containerName);
        Task<string> UploadAsync(string containerName, string fileName, Stream fileContent);
        Task<bool> DeleteIfExistsAsync(string containerName, string fileName);
        Task<Stream> DownloadAsync(string containerName, string blobName);
    }
}
