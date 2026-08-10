

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System.ComponentModel;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading.Tasks;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly string _connectionString ;
        private readonly string _containerName = "states";
        private readonly string _userscontainerName = "users";
        private readonly string _customerscontainerName = "customer";

        private readonly string _dealercontainerName = "dealer";

        private readonly string _blobName = "states.json";
        private readonly string _blobdisticts = "Disticts.json";
        private readonly string _blobcategories = "category.json";
        private readonly string _blobbuildercategorie = "BuilderCategories.json";

        private readonly ICosmosDbService<Customer> _cosmosDbService;

        // Modify the constructor to accept the interface ICosmosDbService<Customer>
       

        public MasterDataController(IConfiguration configuration, ICosmosDbService<Customer> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
            _connectionString = configuration.GetConnectionString("BlobStorage")
                ?? throw new InvalidOperationException("BlobStorage connection string is not configured.");
        }



        // Method to get the list of states
        [HttpGet("getStates")]
        public async Task<IActionResult> GetStates()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(_blobName);

            var response = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(response.Value.Content))
            {
                string jsonContent = await reader.ReadToEndAsync();
                return Ok(jsonContent);
            }
        }


        [HttpGet("getCategories")]
        public async Task<IActionResult> GetCategories()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(_blobcategories);

            var response = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(response.Value.Content))
            {
                string jsonContent = await reader.ReadToEndAsync();
                return Ok(jsonContent);
            }
        }

        [HttpGet("getdealerCategories")]
        public async Task<IActionResult> GetbuilderCategories()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(_blobbuildercategorie);

            var response = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(response.Value.Content))
            {
                string jsonContent = await reader.ReadToEndAsync();
                return Ok(jsonContent);
            }
        }







       




        private static Dictionary<string, UserOnBoarding> _userCache = new Dictionary<string, UserOnBoarding>();
        private static DateTime _lastCacheUpdateTime = DateTime.MinValue;

        // Method to refresh user cache
        private async Task RefreshUserCacheAsync()
        {
            // Only refresh the cache if it's been more than 15 minutes since the last update
            if ((DateTime.Now - _lastCacheUpdateTime).TotalMinutes < 15) return;

            // Clear the existing cache
            _userCache.Clear();

            // Initialize BlobServiceClient and BlobContainerClient for users
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_userscontainerName);

            // Load all users into the cache
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                var blobDownloadInfo = await blobClient.DownloadAsync();

                using (var streamReader = new StreamReader(blobDownloadInfo.Value.Content))
                {
                    string content = await streamReader.ReadToEndAsync();
                    UserOnBoarding user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserOnBoarding>(content);

                    // Cache user by MobileNo, EmailId, and UserId
                    _userCache[user.MobileNo] = user;
                    _userCache[user.EmailId] = user;
                    _userCache[user.UserId.ToString()] = user; // Assuming UserId is of type Guid
                }
            }

            // Update the last cache update time
            _lastCacheUpdateTime = DateTime.Now;
        }


        [HttpGet]
        [Route("VerifyUserProfile")]
        public async Task<IActionResult> GetUser(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BadRequest("Either mobile number or email address must be provided.");
            }

            // Use the GetUserByEmailOrMobileAsync method to fetch the user
            var user = await _cosmosDbService.GetUserByEmailOrMobileAsync(value);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("User not found.");
        }


        [HttpGet("VerifyUserLogin")]
        public async Task<IActionResult> VerifyUserLogin(string username, string password)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                return BadRequest("Either mobile number or email address must be provided.");
            }

            // Use the GetUserByEmailOrMobileAsync method to fetch the user
            var user = await _cosmosDbService.GetUserByLogin(username,password);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("User not found.");
        }



        // Method to get districts by StateId
        [HttpGet("getDistricts/{stateId}")]
        public async Task<IActionResult> GetDistricts(int stateId)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(_blobdisticts);

            var response = await blobClient.DownloadAsync();
            using (StreamReader reader = new StreamReader(response.Value.Content))
            {
                // Read JSON content
                string jsonContent = await reader.ReadToEndAsync();

                // Deserialize the JSON to a list of State objects
                var states = JsonSerializer.Deserialize<List<State>>(jsonContent);

                // Find the state by StateId and get its districts
                var state = states.FirstOrDefault(s => s.StateId == stateId);

                if (state == null)
                {
                    return NotFound(new { Message = "State not found." });
                }

                return Ok(state.Districts);
            }
        }
    }

    // Define classes matching your JSON structure
    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public List<District> Districts { get; set; }
    }

    public class District
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
    }
}
