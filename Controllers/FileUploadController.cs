using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly string _connectionString ;
        private readonly string _containerName = "userattechements";
        //private readonly object _blobServiceClient;
        //private readonly BlobServiceClient _blobServiceClient;
        // private readonly string _containerName = "handymanproducts"; // Change to your container name

        //public FileUploadController(string connectionString)
        //{
        //    connectionString=_connectionString;
        //    //_blobServiceClient = new BlobServiceClient(connectionString);
        //}

        public FileUploadController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BlobStorage")
                ?? throw new InvalidOperationException("BlobStorage connection string is not configured.");
        }



        [HttpPost("upload")]
        public async Task<string> UploadFileAsync([FromForm] string filename, [FromForm] IFormFile file)
        {
            // Create a BlobServiceClient to connect to the Blob service
            BlobServiceClient _blobServiceClient = new BlobServiceClient(_connectionString);

            // Get a reference to the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();
            Guid newGuid = Guid.NewGuid();
            string generatedfilename =  newGuid.ToString() + "_" + filename;
            // Use the filename parameter instead of file.FileName
            var blobClient = containerClient.GetBlobClient(generatedfilename);

            // Upload the file using the provided filename
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Return the URI of the uploaded blob
            return generatedfilename;
        }
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAsync([FromQuery] string generatedfilename)
        {
            // Create a BlobServiceClient to connect to the Blob service
            BlobServiceClient _blobServiceClient = new BlobServiceClient(_connectionString);

            // Get a reference to the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Get a reference to the blob using the generated filename
            var blobClient = containerClient.GetBlobClient(generatedfilename);

            // Check if the blob exists
            if (await blobClient.ExistsAsync())
            {
                // Download the blob's content to a byte array
                var downloadInfo = await blobClient.DownloadAsync();
                using (var stream = downloadInfo.Value.Content)
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();

                    // Return the byte array as a JSON response
                    return Ok(new { imageData = Convert.ToBase64String(imageBytes) });
                }
            }
            else
            {
                return NotFound("File not found.");
            }
        }


        private async Task<string> UploadFileToBlobAsync(IFormFile file)
        {
            // Create a BlobServiceClient to connect to the Blob service
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            // Get a reference to a blob and use the file name
            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Generate unique name for the file
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Return the URI of the uploaded blob
            return blobClient.Uri.ToString();
        }
    }
}
