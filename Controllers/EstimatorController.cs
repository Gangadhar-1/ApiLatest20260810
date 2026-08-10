using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using System.Reflection.Emit;
namespace OtpAuthServices.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class EstimatorController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<Estimator> _cosmosDbService;

        public EstimatorController(BlobService blobService, ICosmosDbService<Estimator> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }

        //[HttpPost]
        //[Route("EstimatorUpload")]
        //public async Task<IActionResult> UploadUserData([FromForm] Estimator estimator)
        //{




        //    estimator.EstimatorId = Guid.NewGuid();
        //    estimator.EstimatorPhotoId = "download (1).jpg";
        //    // Serialize the UserOnBoarding object to JSON
        //    string jsonString = JsonSerializer.Serialize(estimator);

        //    // Create a unique JSON file name based on the username
        //    string jsonFileName = $"{estimator.UserId.ToString().Replace(" ", "_")}.json";

        //    // Upload the JSON content to Azure Blob Storage
        //    using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
        //    {
        //        await _blobService.UploadBlobAsync(jsonFileName, ms, "estimator");
        //    }

        //    return Ok(new { Message = "Estimator data uploaded successfully", JsonFile = jsonFileName });
        //}

        [HttpPost]
        [Route("EstimatorUpload")]
        public async Task<IActionResult> UploadUserData([FromForm] Estimator estimator)
        {
            {
                if (estimator == null)
                {
                    return BadRequest("Product cannot be null.");
                }


                // Ensure the Id is set
                estimator.id = Guid.NewGuid().ToString(); // Assign a new GUID for Id

                estimator.EstimatorId = Guid.NewGuid();
                estimator.Status = "Pending";
                estimator.IsActive = true;  

                await _cosmosDbService.AddItemAsync(estimator);
                return Ok(new { Message = "Estimator data Inserted successfully", JsonFile = estimator.id });
            }

        }


        [HttpGet("estimatorProfileData")]
        public async Task<IActionResult> estimatorProfileData(string profileType, string UserId)
        {
            try
            {

                ProfileData profileData = new ProfileData();
                string sanitizedProfileType = profileType.ToLower();



                if (string.IsNullOrEmpty(UserId))
                {
                    return BadRequest("User Id cannot be empty");
                }


                var user = await _cosmosDbService.GetEstimatorProflie(UserId, profileType);

                if (user != null)
                {




                    // Extract FirstName and LastName safely
                    profileData.FullName = user.EstimatorFirmName;
                    profileData.MobileNumber = user.PhoneNumber;


                    // Extract Email and MobileNumber safely
                    profileData.Email = user.EmailAddress;

                    // string capitalizedProfileType = char.ToUpper(profileType[0]) + profileType.Substring(1);




                    profileData.PhotoAttachmentId = user.EstimatorPhotoId;





                    profileData.Address = user.Address;
                    profileData.UserId = UserId;
                    profileData.UserProfileType = profileType;








                    return Ok(profileData);
                }
                else
                {
                    return NotFound(new { message = "Estimator not found" });
                }
            }

            catch (Exception ex)
            {
                // Handle exceptions (for example, log the error and return an internal server error)
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("EstimatorEdit")]
        public async Task<IActionResult> EditUserData(string UserId, string FullName = null, string PhotoDocumentId = null)
        {
            // Validate incoming data
            if (UserId == null)
            {
                return BadRequest($"Estimator information is incorrect or {UserId} mismatch.");
            }

            // Fetch existing customer data from Cosmos DB
            Estimator existingEstimator = null;
            try
            {
                // Assuming you are using the Cosmos DB container to fetch the existing customer by id
                existingEstimator = await _cosmosDbService.GetEstimatorProflie(UserId, "Estimator");  // Assuming this is a method to get the customer by id

                if (existingEstimator == null)
                {
                    return NotFound($"Estimator with ID {UserId} not found.");
                }
            }
            catch (CosmosException ex)
            {
                // Log CosmosException (error accessing Cosmos DB)
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }

            // Now that we have the existing customer, we can update the necessary fields
            existingEstimator.EstimatorFirmName = FullName;
            // existingCustomer.LastName = "";
            // Example field update
            //  existingCustomer.LastName =LastName ?? existingCustomer.LastName;
            //existingCustomer.EmailAddress = customer.EmailAddress ?? existingCustomer.EmailAddress;
            existingEstimator.EstimatorPhotoId = PhotoDocumentId;
            // Update the existing customer in Cosmos DB
            try
            {
                // Use UpsertItemAsync to update or insert the customer data
                await _cosmosDbService.UpdateItemAsync(existingEstimator);

                return Ok(existingEstimator);  // Return the updated customer data
            }
            catch (Exception ex)
            {
                // Log general exception (other errors)
                return StatusCode(500, "An error occurred while updating customer data.");
            }
        }

        [HttpGet("GetAllEstimators")]

        public async Task<IActionResult> GetAllEstimators()
        {
            try
            {
                var data = await _cosmosDbService.GetAllEstimatorsDetails();

               
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

        [HttpGet("GetEstimatorDetailsByUserId")]
        public async Task<IActionResult> GetEstimatorDetails(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("UserId cannot be null or empty.");
                }

                var data = await _cosmosDbService.GetEstimatorDetailsByUserId(userId);


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



        [HttpGet("EstimatorDirectoryDetails")]
        public async Task<IActionResult> EstimatorDirectoryDetails(string searchQuery = null, string State = null, string District = null, string ZipCode=null,string Status = null)
        {
            try
            {
                Console.WriteLine($"Inputs - SearchQuery: {searchQuery}, State: {State}, District: {District},ZipCode:{ZipCode},Status:{Status}");
                var data = await _cosmosDbService.GetEstimatorDirectoryDetails(searchQuery, State, District,ZipCode,Status);

                if (data == null || !data.Any())
                {
                    return NotFound("No data found.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpGet("{estimatorId:guid}")]
        public async Task<IActionResult> GetEstimatorById(Guid estimatorId)
        {
            try
            {
                var results = await _cosmosDbService.GetEstimatorByIdAsync<Estimator>(estimatorId);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "Estimator not found." });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }


        [HttpPut("Status/{EstimatorId}")]
        public async Task<IActionResult> UpdateEstimatorStatus(Guid EstimatorId, [FromQuery] string Status)
        {
            // Fetch the dealer details by ID from the database
            var EstimatorList = await _cosmosDbService.GetEstimatorByIdAsync<Estimator>(EstimatorId);

            if (EstimatorList == null || EstimatorList.Count == 0)
            {
                return NotFound(new { message = "Estimator not found.", isSuccess = false });
            }

            // Assuming only one dealer should be returned, take the first one
            var estimator = EstimatorList.FirstOrDefault();

            if (estimator == null)
            {
                return NotFound(new { message = "Estimator not found.", isSuccess = false });
            }

            // Exit early if the status is already the same
            if ((string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase) && estimator.Status == "Approved") ||
                (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase) && estimator.Status == "Rejected") ||
                (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase) && estimator.Status == "Pending"))
            {
                return Ok(new { message = "No changes made. Status is already the same.", isSuccess = true });
            }

            // Update the dealer's status based on the Status parameter
            if (string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                estimator.IsApproved = true;
                estimator.IsRejected = false;
                estimator.IsPending = false;
                estimator.Status = "Approved"; // Update the Status field
            }
            else if (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                estimator.IsApproved = false;
                estimator.IsRejected = true;
                estimator.IsPending = false;
                estimator.Status = "Rejected"; // Update the Status field
            }
            else if (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                estimator.IsPending = true;
                estimator.IsRejected = false;
                estimator.IsApproved = false;
                estimator.Status = "Pending"; // Update the Status field
            }
            else
            {
                return BadRequest(new { message = "Invalid status. Please use 'Approved', 'Rejected', or 'Pending'." });
            }

            // Save the updated dealer back to the Cosmos DB
            await _cosmosDbService.UpdateEstimatorAsync(estimator);
            return Ok(new
            {
                message = $"Estimator status updated to {estimator.Status}.",
                isSuccess = true
            });
        }






        [HttpPut("UpdateIsActive/{estimaorId}")]
        public async Task<IActionResult> UpdateIsActive(Guid estimaorId, [FromBody] bool isActive)
        {
            if (estimaorId == Guid.Empty)
            {
                return BadRequest(false); // Return false for invalid technician ID
            }

            // Fetch technician by ID
            var estimators = await _cosmosDbService.GetEstimatorByIdAsync<Estimator>(estimaorId);

            if (estimators == null || !estimators.Any())
            {
                return NotFound(false); // Return false if technician not found
            }

            var estimator = estimators.First();

            if (estimator.IsActive == isActive)
            {
                return Ok(false); // Return true if no update is needed
            }

            // Update the IsActive status
            estimator.IsActive = isActive;

            // Call the update method and check for success
            var updateSuccess = await _cosmosDbService.UpdateEstimatorAsync(estimator);

            if (updateSuccess)
            {
                return Ok(true); // Return true if update was successful
            }

            // If update fails, return false
            return Ok(false);
        }








    }







}
        


//        [HttpPost]
//        [Route("EstimatorEdit")]
//        public async Task<IActionResult> EditUserData(string UserId, string FullName=null, string PhotoDocumentId=null)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(UserId))
//                {
//                    return BadRequest(new { Message = "UserId is required." });
//                }

//                // Sanitize UserId to avoid invalid characters in the file name
//                string sanitizedUserId = string.Join("_", UserId.Split(Path.GetInvalidFileNameChars()));
//                string jsonFileName = $"{sanitizedUserId}.json";

//                // Download the existing estimator data
//                Stream existingDataStream = await _blobService.DownloadBlobAsync(jsonFileName.Replace("'", ""), "estimator");

//                if (existingDataStream == null)
//                {
//                    return NotFound(new { Message = $"Estimator data with UserId {UserId} not found." });
//                }

//                // Read the existing data
//                string existingData;
//                using (StreamReader reader = new StreamReader(existingDataStream))
//                {
//                    existingData = await reader.ReadToEndAsync();
//                }

//                // Deserialize the existing data into the Estimator object
//                Estimator existingEstimator;
//                try
//                {
//                    existingEstimator = JsonSerializer.Deserialize<Estimator>(existingData);
//                }
//                catch (JsonException jsonEx)
//                {
//                    return StatusCode(500, new { Message = "Failed to parse estimator data.", Error = jsonEx.Message });
//                }

//                // Update the fields only if the new values are provided
//                if (!string.IsNullOrEmpty(FullName))
//                {
//                    existingEstimator.EstimatorName = FullName.Replace("'", "");
//                }
//                if (!string.IsNullOrEmpty(PhotoDocumentId)) 
//                existingEstimator.EstimatorPhotoId=PhotoDocumentId.Replace("'", "");

//                // Serialize the updated Estimator object
//                string updatedJsonString = JsonSerializer.Serialize(existingEstimator);

//                // Upload the updated data back to the correct container ("estimator")
//                using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(updatedJsonString)))
//                {
//                    await _blobService.UploadBlobAsync(jsonFileName, ms, "estimator");
//                }

//                return Ok(new { Message = "Estimator data updated successfully", JsonFile = jsonFileName });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while updating estimator data.", Error = ex.Message });
//            }
//        }
//    }
//}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration.UserSecrets;
//using System.Data;
//using System.Data.SqlClient;
//using System.Threading.Tasks;

//[Route("api/[controller]")]
//[ApiController]
//public class EstimatorController : ControllerBase
//{
//    private readonly string _connectionString;

//    public EstimatorController(IConfiguration configuration)
//    {
//        _connectionString = configuration.GetConnectionString("DefaultConnection");
//    }

//    [HttpPost]
//    [Route("InsertOrUpdateEstimator")]
//    public async Task<IActionResult> InsertOrUpdateEstimator([FromBody] OtpAuthServices.Model.EstimatorDto estimator)
//    {
//        if (estimator == null)
//        {
//            return BadRequest("Estimator data is required.");
//        }

//        string message;
//        using (SqlConnection conn = new SqlConnection(_connectionString))
//        {
//            using (SqlCommand cmd = new SqlCommand("Usp_InsertOrUpdateEstimator", conn))
//            {
//                cmd.CommandType = CommandType.StoredProcedure;

//                cmd.Parameters.AddWithValue("@FirstName", estimator.FirstName ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@LastName", estimator.LastName ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@EstimatorFirmName", estimator.EstimatorFirmName);
//                cmd.Parameters.AddWithValue("@EstimatorFirmRegistrationNumber", estimator.EstimatorFirmRegistrationNumber ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@FirmRegistrationForm", estimator.FirmRegistrationForm ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@GSTNumber", estimator.GSTNumber ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@PANNumber", estimator.PANNumber);
//                cmd.Parameters.AddWithValue("@PanCard", estimator.PanCard ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@AadharNumber", estimator.AadharNumber);
//                cmd.Parameters.AddWithValue("@Address", estimator.Address);
//                cmd.Parameters.AddWithValue("@State", estimator.State);
//                cmd.Parameters.AddWithValue("@District", estimator.District);
//                cmd.Parameters.AddWithValue("@ZipCode", estimator.ZipCode);
//                cmd.Parameters.AddWithValue("@PhoneNumber", estimator.PhoneNumber ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@PhoneVerificationCode", estimator.PhoneVerificationCode ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@EmailAddress", estimator.EmailAddress ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@EmailVerificationCode", estimator.EmailVerificationCode ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@EstimatorPhoto", estimator.EstimatorPhoto ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@Category", estimator.Category ?? (object)DBNull.Value);
//cmd.Parameters.AddWithValue("@UserId", estimator.UserId == Guid.Empty ? (object)DBNull.Value : estimator.UserId);
//                SqlParameter messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 600)
//                {
//                    Direction = ParameterDirection.Output
//                };
//                cmd.Parameters.Add(messageParam);

//                await conn.OpenAsync();
//                await cmd.ExecuteNonQueryAsync();
//                message = messageParam.Value.ToString();
//            }
//        }

//        return Ok(new { Message = message });
//    }
//}
