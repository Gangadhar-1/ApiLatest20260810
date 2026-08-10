using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;

namespace OtpAuthServices.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BuilderController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<Builder> _cosmosDbService;

        public BuilderController(BlobService blobService, ICosmosDbService<Builder> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }

        //[HttpPost]
        //[Route("BuilderUpload")]
        //public async Task<IActionResult> UploadUserData([FromForm] Builder builder)
        //{

        //   builder.BuilderId = Guid.NewGuid();
        //    builder.BuilderPhotoId = "download (1).jpg";
        //    // Serialize the UserOnBoarding object to JSON
        //    string jsonString = JsonSerializer.Serialize(builder);

        //    // Create a unique JSON file name based on the username
        //    string jsonFileName = $"{builder.UserId.ToString().Replace(" ", "_")}.json";

        //    // Upload the JSON content to Azure Blob Storage
        //    using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
        //    {
        //        await _blobService.UploadBlobAsync(jsonFileName, ms, "builder");
        //    }

        //    return Ok(new { Message = "Builder data uploaded successfully", JsonFile = jsonFileName });
        //}



        [HttpPost]
        [Route("BuilderUpload")]

        public async Task<IActionResult> UploadUserData([FromForm] Builder builder)
        {
            if (builder == null)
            {
                return BadRequest("Product cannot be null.");
            }


            // Ensure the Id is set
            builder.id = Guid.NewGuid().ToString(); // Assign a new GUID for Id

            builder.BuilderId = Guid.NewGuid();
            builder.Status = "Pending";
            builder.IsActive = true;    
            await _cosmosDbService.AddItemAsync(builder);
            return Ok(new { Message = "Builder data Inserted successfully", JsonFile = builder.id });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilderDetails(string id, [FromBody] Builder builder)
        {
            if (builder == null || builder.id != id)
            {
                return BadRequest("builder information is incorrect.");
            }

            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(builder);
            return Ok("builder data Updated successfully.");
        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteBuilderDetails(string id)
        {
            var existingbuilder = await _cosmosDbService.GetItemsAsync(id);
                if(existingbuilder ==null)
            {
                return NotFound("builder details not found");
            }
            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Builder details deleted successfully");

        }


        [HttpGet("builderProfileData")]
        public async Task<IActionResult> BuilderProfileData(string profileType, string UserId)
        {
            try
            {

                ProfileData profileData = new ProfileData();
                string sanitizedProfileType = profileType.ToLower();



                if (string.IsNullOrEmpty(UserId))
                {
                    return BadRequest("User Id cannot be empty");
                }


                var user = await _cosmosDbService.GetBuilderProflie(UserId, profileType);

                if (user != null)
                {
                    // Extract FirstName and LastName safely
                    profileData.FullName = user.BuilderFirmName;
                    profileData.MobileNumber = user.PhoneNumber;
                    // Extract Email and MobileNumber safely
                    profileData.Email = user.EmailAddress;

                    // string capitalizedProfileType = char.ToUpper(profileType[0]) + profileType.Substring(1);

                    profileData.PhotoAttachmentId = user.BuilderPhotoId;
                    profileData.Address = user.Address;
                    profileData.UserId = UserId;
                    profileData.UserProfileType = profileType;
                    return Ok(profileData);
                }
                else
                {
                    return NotFound(new { message = "Dealer not found" });
                }
            }

            catch (Exception ex)
            {
                // Handle exceptions (for example, log the error and return an internal server error)
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("BuilderEdit")]
        public async Task<IActionResult> EditUserData(string UserId, string FullName = null, string PhotoDocumentId = null)
        {
            // Validate incoming data
            if (UserId == null)
            {
                return BadRequest($"Customer information is incorrect or {UserId} mismatch.");
            }

            // Fetch existing customer data from Cosmos DB
            Builder existingCustomer = null;
            try
            {
                // Assuming you are using the Cosmos DB container to fetch the existing customer by id
                existingCustomer = await _cosmosDbService.GetBuilderProflie(UserId, "Customer");  // Assuming this is a method to get the customer by id

                if (existingCustomer == null)
                {
                    return NotFound($"Customer with ID {UserId} not found.");
                }
            }
            catch (CosmosException ex)
            {
                // Log CosmosException (error accessing Cosmos DB)
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }

            // Now that we have the existing customer, we can update the necessary fields
            existingCustomer.BuilderName = FullName;
            // existingCustomer.LastName = "";
            // Example field update
            //  existingCustomer.LastName =LastName ?? existingCustomer.LastName;
            //existingCustomer.EmailAddress = customer.EmailAddress ?? existingCustomer.EmailAddress;
            existingCustomer.BuilderPhotoId = PhotoDocumentId;
            // Update the existing customer in Cosmos DB
            try
            {
                // Use UpsertItemAsync to update or insert the customer data
                await _cosmosDbService.UpdateItemAsync(existingCustomer);

                return Ok(existingCustomer);  // Return the updated customer data
            }
            catch (Exception ex)
            {
                // Log general exception (other errors)
                return StatusCode(500, "An error occurred while updating customer data.");
            }
        }

        [HttpGet("GetAllBuildersDetails")]

        public async Task<IActionResult>   GetAllBuildersDetails()
        {

            try
            {

                var data = await _cosmosDbService.GetAllBuildersDetails();
               
                return Ok(data);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetBuilderDetailsByUserId")]
        public async Task<IActionResult> GetBuilderDetails(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("UserId cannot be null or empty.");
                }

                var data = await _cosmosDbService.GetBuilderDetailsByUserId(userId);


                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }


        [HttpGet("BuilderDirectoryDetails")]
        public async Task<IActionResult> BuilderDirectoryDetails(
            string searchQuery = null,
            string State = null,
            string District = null,
            string ZipCode = null,
            string Status=null)
        {
            try
            {
                Console.WriteLine($"Inputs - SearchQuery: {searchQuery}, State: {State}, District: {District}, ZipCode: {ZipCode},Status:{Status}");

                // Fetch data from the service with the provided filters
                var data = await _cosmosDbService.GetBuilderDirectoryDetails(searchQuery, State, District, ZipCode,Status);

                // Check if any data is returned
                if (data == null || !data.Any())
                {
                    Console.WriteLine("No data found for the provided filters.");
                    return NotFound("No data found.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        [HttpGet("{builderId:guid}")]
        public async Task<IActionResult> GetBuilderById(Guid builderId)
        {
            try
            {
                // Explicitly specify the type, e.g., 'Dealer'
                var results = await _cosmosDbService.GetBuilderByIdAsync<Builder>(builderId);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "Builder not found." });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }


        [HttpPut("Status/{BuilderId}")]
        public async Task<IActionResult> UpdateBuilderStatus(Guid BuilderId, [FromQuery] string Status)
        {
            // Fetch the dealer details by ID from the database
            var builderList = await _cosmosDbService.GetBuilderByIdAsync<Builder>(BuilderId);

            if (builderList == null || builderList.Count == 0)
            {
                return NotFound(new { message = "builder not found.", isSuccess = false });
            }

            // Assuming only one dealer should be returned, take the first one
            var builder = builderList.FirstOrDefault();

            if (builder == null)
            {
                return NotFound(new { message = "builder not found.", isSuccess = false });
            }

            // Exit early if the status is already the same
            if ((string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase) && builder.Status == "Approved") ||
                (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase) && builder.Status == "Rejected") ||
                (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase) && builder.Status == "Pending"))
            {
                return Ok(new { message = "No changes made. Status is already the same.", isSuccess = true });
            }

            // Update the dealer's status based on the Status parameter
            if (string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                builder.IsApproved = true;
                builder.IsRejected = false;
                builder.IsPending = false;
                builder.Status = "Approved"; // Update the Status field
            }
            else if (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                builder.IsApproved = false;
                builder.IsRejected = true;
                builder.IsPending = false;
                builder.Status = "Rejected"; // Update the Status field
            }
            else if (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                builder.IsPending = true;
                builder.IsRejected = false;
                builder.IsApproved = false;
                builder.Status = "Pending"; // Update the Status field
            }
            else
            {
                return BadRequest(new { message = "Invalid status. Please use 'Approved', 'Rejected', or 'Pending'." });
            }

            // Save the updated dealer back to the Cosmos DB
            await _cosmosDbService.UpdateBuilderAsync(builder);

            return Ok(new
            {
                message = $"builder status updated to {builder.Status}.",
                isSuccess = true
            });
        }


        [HttpPut("UpdateIsActive/{builderId}")]
        public async Task<IActionResult> UpdateIsActive(Guid builderId, [FromBody] bool isActive)
        {
            if (builderId == Guid.Empty)
            {
                return BadRequest(false); // Return false for invalid technician ID
            }

            // Fetch technician by ID
            var builders = await _cosmosDbService.GetBuilderByIdAsync<Builder>(builderId);

            if (builders == null || !builders.Any())
            {
                return NotFound(false); // Return false if technician not found
            }

            var builder = builders.First();

            if (builder.IsActive == isActive)
            {
                return Ok(false); // Return true if no update is needed
            }

            // Update the IsActive status
            builder.IsActive = isActive;

            // Call the update method and check for success
            var updateSuccess = await _cosmosDbService.UpdateBuilderAsync(builder);

            if (updateSuccess)
            {
                return Ok(true); // Return true if update was successful
            }

            // If update fails, return false
            return Ok(false);
        }




    }
}


//    [HttpPost]
//        [Route("BuilderEdit")]
//        public async Task<IActionResult> EditUserData(string UserId, string FullName=null, string PhotoDocumentId=null)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(UserId))
//                {
//                    return BadRequest(new { Message = "UserId is required." });
//                }

//                string jsonFileName = $"{UserId.ToString().Replace(" ", "_").Replace("'","")}.json";
//                Stream existingDataStream = await _blobService.DownloadBlobAsync(jsonFileName, "builder");

//                if (existingDataStream == null)
//                {
//                    return NotFound(new { Message = $"Builder data with UserId {UserId} not found." });
//                }

//                string existingData;
//                using (StreamReader reader = new StreamReader(existingDataStream))
//                {
//                    existingData = await reader.ReadToEndAsync();
//                }

//                Builder existingBuilder;
//                try
//                {
//                    existingBuilder = JsonSerializer.Deserialize<Builder>(existingData);
//                }
//                catch (JsonException jsonEx)
//                {
//                    return StatusCode(500, new { Message = "Failed to parse customer data.", Error = jsonEx.Message });
//                }

//                //existingBuilder.BuilderName = FullName.ToString() ?? existingBuilder.BuilderName;
//                // existingCustomer.LastName = FullName.Split("$")[1].ToString() ?? existingCustomer.LastName;

//                if (!string.IsNullOrEmpty(FullName))
//                { 
//                    existingBuilder.BuilderName = FullName.Replace("'", ""); 
//                }
                  

//                if (!string.IsNullOrEmpty(PhotoDocumentId))
//                { 
//                    existingBuilder.BuilderPhotoId = PhotoDocumentId.Replace("'", "");
//                }
                    
//                              string updatedJsonString = JsonSerializer.Serialize(existingBuilder);
//                using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(updatedJsonString)))
//                {
//                    await _blobService.UploadBlobAsync(jsonFileName, ms, "builder");
//                }

//                return Ok(new { Message = "Builder data updated successfully", JsonFile = jsonFileName });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while updating customer data.", Error = ex.Message });
//            }
//        }
//    }
//}






//using System;
//using System.Data;
//using System.Data.SqlClient;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;

//namespace OtpAuthServices.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BuilderController : ControllerBase
//    {
//        private readonly IConfiguration _configuration;
//        private readonly string _connectionString;

//        public BuilderController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//            _connectionString = _configuration.GetConnectionString("DefaultConnection");
//        }

//        [HttpPost("InsertOrUpdateBuilder")]
//        public async Task<IActionResult> InsertOrUpdateBuilder([FromBody] Builder builder)
//        {
//            if (builder == null)
//            {
//                return BadRequest("Builder data is null");
//            }

//            try
//            {
//                using (SqlConnection conn = new SqlConnection(_connectionString))
//                {
//                    await conn.OpenAsync();

//                    using (SqlCommand cmd = new SqlCommand("Usp_InsertOrUpdateBuilder", conn))
//                    {
//                        cmd.CommandType = CommandType.StoredProcedure;

//                        // Input parameters
//                        cmd.Parameters.AddWithValue("@BuilderFirmName", builder.BuilderFirmName);
//                        cmd.Parameters.AddWithValue("@BuilderFirmRegistrationNumber", builder.BuilderFirmRegistrationNumber);
//                        cmd.Parameters.AddWithValue("@BuilderName", builder.BuilderName);
//                        cmd.Parameters.AddWithValue("@PANNumber", builder.PanNumber);
//                        cmd.Parameters.AddWithValue("@PanCard", (object)builder.PanCard ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@AadharNumber", builder.AadharNumber);
//                        cmd.Parameters.AddWithValue("@GSTNumber", (object)builder.GSTNumber ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Address", builder.Address);
//                        cmd.Parameters.AddWithValue("@State", builder.State);
//                        cmd.Parameters.AddWithValue("@District", builder.District);
//                        cmd.Parameters.AddWithValue("@ZipCode", builder.ZipCode);
//                        cmd.Parameters.AddWithValue("@PhoneNumber", builder.PhoneNumber);
//                        cmd.Parameters.AddWithValue("@PhoneVerificationCode", (object)builder.PhoneVerificationCode ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@EmailAddress", builder.EmailAddress);
//                        cmd.Parameters.AddWithValue("@EmailVerificationCode", (object)builder.EmailVerificationCode ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@BuilderPhoto", (object)builder.BuilderPhoto ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Category", (object)builder.Category ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@UserId", builder.UserId);

//                        // Output parameter for the message
//                        SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.VarChar, 600)
//                        {
//                            Direction = ParameterDirection.Output
//                        };
//                        cmd.Parameters.Add(outputMessage);

//                        // Execute the stored procedure
//                        await cmd.ExecuteNonQueryAsync();

//                        // Get the output message
//                        string message = outputMessage.Value.ToString();

//                        // Return the message as the response
//                        return Ok(new { Message = message });
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }
//    }
//}
