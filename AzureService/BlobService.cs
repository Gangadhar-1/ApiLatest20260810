    namespace OtpAuthServices.AzureService
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "handymanproducts"; // Change to your container name

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Uploads a blob to the specified container
        public async Task UploadBlobAsync(string blobName, MemoryStream ms, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);
            ms.Position = 0; // Reset stream position before uploading
            await blobClient.UploadAsync(ms, true);
        }

        // Downloads a blob from the specified container
        public async Task<Stream> DownloadBlobAsync(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            MemoryStream ms = new MemoryStream();
            await download.Content.CopyToAsync(ms);
            ms.Position = 0; // Reset stream position for reading

            return ms;
        }

        // Retrieves the URL of the blob in the specified container
        public string GetBlobUrl(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            return blobClient.Uri.ToString();
        }
    }
}
