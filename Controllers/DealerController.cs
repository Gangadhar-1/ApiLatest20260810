using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Azure.Cosmos;
using Twilio.TwiML.Messaging;

namespace OtpAuthServices.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DealerController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<Dealer> _cosmosDbService;

        public DealerController(BlobService blobService, ICosmosDbService<Dealer> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }

        //[HttpPost]
        //[Route("DealerUpload")]
        //public async Task<IActionResult> UploadUserData([FromForm] Dealer dealer)
        //{




        //    dealer.DealerId = Guid.NewGuid();
        //    dealer.DealerPhotoId = "download (1).jpg";
        //    // Serialize the UserOnBoarding object to JSON
        //    string jsonString = JsonSerializer.Serialize(dealer);

        //    // Create a unique JSON file name based on the username
        //    string jsonFileName = $"{dealer.UserId.ToString().Replace(" ", "_")}.json";

        //    // Upload the JSON content to Azure Blob Storage
        //    using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
        //    {
        //        await _blobService.UploadBlobAsync(jsonFileName, ms, "dealer");
        //    }

        //    return Ok(new { Message = "Dealer data Inserted successfully", JsonFile = jsonFileName });
        //}

        [HttpPost]
        [Route("DealerUpload")]
        public async Task<IActionResult> UploadUserData([FromForm] Dealer dealer)
        {
            {
                if (dealer == null)
                {
                    return BadRequest("Product cannot be null.");
                }


                // Ensure the Id is set
                dealer.id = Guid.NewGuid().ToString(); // Assign a new GUID for Id
               dealer.DealerPhotoId = "download (1).jpg";

                dealer.DealerId = Guid.NewGuid();
                dealer.Status = "Pending";
                dealer.IsActive = true; 
                //dealer.Status = new Status
                //{
                //    status = "Pending",
                //    UpdatedAt = DateTime.UtcNow,  
                //    CreatedDate = DateTime.UtcNow, 
                //    ModifiedDate = DateTime.UtcNow 
                //};


                await _cosmosDbService.AddItemAsync(dealer);
                return Ok(new { Message = "Dealer data Inserted successfully", JsonFile = dealer.id });
            }

        }





        [HttpGet("dealerProfileData")]
        public async Task<IActionResult> dealerProfileData(string profileType, string UserId)
        {
            try
            {

                ProfileData profileData = new ProfileData();
                string sanitizedProfileType = profileType.ToLower();



                if (string.IsNullOrEmpty(UserId))
                {
                    return BadRequest("User Id cannot be empty");
                }


                var user = await _cosmosDbService.GetDealerProflie(UserId, profileType);

                if (user != null)
                {




                    // Extract FirstName and LastName safely
                    profileData.FullName = user.DealerFirmName;
                    profileData.MobileNumber = user.PhoneNumber;

                    profileData.Category = user.Category;

                    profileData.District = user.District;


                    // Extract Email and MobileNumber safely
                    profileData.Email = user.EmailAddress;

                    // string capitalizedProfileType = char.ToUpper(profileType[0]) + profileType.Substring(1);




                    profileData.PhotoAttachmentId = user.DealerPhotoId;





                    profileData.Address = user.Address;
                    profileData.UserId = UserId;
                    profileData.UserProfileType = profileType;

                    profileData.Status = user.Status;
                    profileData.IsActive = user.IsActive;







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
        [Route("DealerEdit")]
        public async Task<IActionResult> EditUserData(string UserId, string FullName = null, string PhotoDocumentId = null)
        {
            // Validate incoming data
            if (UserId == null)
            {
                return BadRequest($"Dealer information is incorrect or {UserId} mismatch.");
            }

            // Fetch existing customer data from Cosmos DB
            Dealer existingDealer = null;
            try
            {
                // Assuming you are using the Cosmos DB container to fetch the existing customer by id
                existingDealer = await _cosmosDbService.GetDealerProflie(UserId, "Dealer");  // Assuming this is a method to get the customer by id

                if (existingDealer == null)
                {
                    return NotFound($"Dealer with ID {UserId} not found.");
                }
            }
            catch (CosmosException ex)
            {
                // Log CosmosException (error accessing Cosmos DB)
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }

            // Now that we have the existing customer, we can update the necessary fields
            existingDealer.DealerFirmName = FullName;
            // existingCustomer.LastName = "";
            // Example field update
            //  existingCustomer.LastName =LastName ?? existingCustomer.LastName;
            //existingCustomer.EmailAddress = customer.EmailAddress ?? existingCustomer.EmailAddress;
            existingDealer.DealerPhotoId = PhotoDocumentId;
            // Update the existing customer in Cosmos DB
            try
            {
                // Use UpsertItemAsync to update or insert the customer data
                await _cosmosDbService.UpdateItemAsync(existingDealer);

                return Ok(existingDealer);  // Return the updated customer data
            }
            catch (Exception ex)
            {
                // Log general exception (other errors)
                return StatusCode(500, "An error occurred while updating customer data.");
            }
        }

        [HttpGet("GetAllDealerDetails")]

        public async Task<IActionResult> GetAllDealerDetails()
        {

            try
            {

                var data = await _cosmosDbService.GetAllDealersDetails();

                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetDealerDtailsByUserId")]
        public async Task<IActionResult> GetDealerDetails(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("UserId cannot be null or empty.");
                }

                var data = await _cosmosDbService.GetDealerDetailsByUserId(userId);


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


        [HttpGet("DealerDirectoryDetails")]
        public async Task<IActionResult> DealerDirectoryDetails(
    string searchQuery = null,
    string State = null,
    string District = null,
    string ZipCode = null,
    string Status = null)
        {
            try
            {
                // Call the service to fetch dealer details
                var dealers = await _cosmosDbService.GetDealerDirectoryDetails(searchQuery, State, District, ZipCode,Status);

                // If no dealers found, return NotFound
                if (dealers == null || dealers.Count == 0)
                {
                    return NotFound("No dealers found with the specified filters.");
                }

                // Return the results
                return Ok(dealers);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error retrieving dealer details: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{dealerId:guid}")]
        public async Task<IActionResult> GetDealerById(Guid dealerId)
        {
            try
            {
                // Explicitly specify the type, e.g., 'Dealer'
                var results = await _cosmosDbService.GetDealerByIdAsync<Dealer>(dealerId);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "Dealer not found." });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }


        [HttpPut("Status/{DealerId}")]
        public async Task<IActionResult> UpdateDealerStatus(Guid DealerId, [FromQuery] string Status)
        {
            // Fetch the dealer details by ID from the database
            var dealerList = await _cosmosDbService.GetDealerByIdAsync<Dealer>(DealerId);

            if (dealerList == null || dealerList.Count == 0)
            {
                return NotFound(new { message = "Dealer not found.", isSuccess = false });
            }

            // Assuming only one dealer should be returned, take the first one
            var dealer = dealerList.FirstOrDefault();

            if (dealer == null)
            {
                return NotFound(new { message = "Dealer not found.", isSuccess = false });
            }

            // Exit early if the status is already the same
            if ((string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase) && dealer.Status == "Approved") ||
                (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase) && dealer.Status == "Rejected") ||
                (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase) && dealer.Status == "Pending"))
            {
                return Ok(new { message = "No changes made. Status is already the same.", isSuccess = true });// Exit without updating
            }

            // Update the dealer's status based on the Status parameter
            if (string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                dealer.IsApproved = true;
                dealer.IsRejected = false;
                dealer.IsPending = false;
                dealer.Status = "Approved"; // Update the Status field
            }
            else if (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                dealer.IsApproved = false;
                dealer.IsRejected = true;
                dealer.IsPending = false;
                dealer.Status = "Rejected"; // Update the Status field
            }
            else if (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                dealer.IsPending = true;
                dealer.IsRejected = false;
                dealer.IsApproved = false;
                dealer.Status = "Pending"; // Update the Status field
            }
            else
            {
                return BadRequest(new { message = "Invalid status. Please use 'Approved', 'Rejected', or 'Pending'." });
            }

            // Save the updated dealer back to the Cosmos DB
            await _cosmosDbService.UpdateDealerAsync(dealer);

            return Ok(new
            {
                message = $"Dealer status updated to {dealer.Status}.",
                isSuccess = true
            });
        }


        [HttpPut("UpdateIsActive/{dealerId}")]
        public async Task<IActionResult> UpdateIsActive(Guid dealerId, [FromBody] bool isActive)
        {
            if (dealerId == Guid.Empty)
            {
                return BadRequest(false); // Return false for invalid technician ID
            }

            // Fetch technician by ID
            var dealers = await _cosmosDbService.GetDealerByIdAsync<Dealer>(dealerId);

            if (dealers == null || !dealers.Any())
            {
                return NotFound(false); // Return false if technician not found
            }

            var dealer = dealers.First();

            if (dealer.IsActive == isActive)
            {
                return Ok(false); // Return true if no update is needed
            }

            // Update the IsActive status
            dealer.IsActive = isActive;

            // Call the update method and check for success
            var updateSuccess = await _cosmosDbService.UpdateDealerAsync(dealer);

            if (updateSuccess)
            {
                return Ok(true); // Return true if update was successful
            }

            // If update fails, return false
            return Ok(false);
        }


        [HttpGet("GetDealerDetailsForInvoice")]
        public async Task<ActionResult> GetDealerDetailsForInvoice(string DealerId)
        {
            try
            {

                // Fetch the total count from the service
                var dealerInvoice = await _cosmosDbService.GetDealerDetailsForInvoice(DealerId);

                if (dealerInvoice == null)
                {
                    return StatusCode(500, "Error fetching total counts .");
                }
                return Ok(dealerInvoice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected  error occurred.");
            }
        }


        [HttpGet("GetDealerMobileAndEmail")]
        public async Task<ActionResult> GetDealerMobileAndEmail( string District)
        {
            try
            {
                var technicianMobileAndEmails = await _cosmosDbService.GetDealerMobileAndEmail( District);

                if (technicianMobileAndEmails == null || !technicianMobileAndEmails.Any())
                {
                    return NotFound("No data found.");
                }

                return Ok(technicianMobileAndEmails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }

    }
}











//[HttpPost]
//[Route("DealerEdit")]
//public async Task<IActionResult> EditUserData(string UserId, string FullName, string PhotoDocumentId)
//{
//    try
//    {
//        if (string.IsNullOrEmpty(UserId))
//        {
//            return BadRequest(new { Message = "UserId is required." });
//        }

//        string jsonFileName = $"{UserId.ToString().Replace(" ", "_").Replace("'","")}.json";
//        Stream existingDataStream = await _blobService.DownloadBlobAsync(jsonFileName, "dealer");

//        if (existingDataStream == null)
//        {
//            return NotFound(new { Message = $"Dealer data with UserId {UserId} not found." });
//        }

//        string existingData;
//        using (StreamReader reader = new StreamReader(existingDataStream))
//        {
//            existingData = await reader.ReadToEndAsync();
//        }

//        Dealer existingDealer;
//        try
//        {
//            existingDealer = JsonSerializer.Deserialize<Dealer>(existingData);
//        }
//        catch (JsonException jsonEx)
//        {
//            return StatusCode(500, new { Message = "Failed to parse dealer data.", Error = jsonEx.Message });
//        }

//                //existingDealer.OwnershipName = FullName.ToString() ?? existingDealer.OwnershipName;
//                // existingCustomer.LastName = FullName.Split("$")[1].ToString() ?? existingCustomer.LastName;

//                if (!string.IsNullOrEmpty(FullName))
//                { existingDealer.OwnershipName = FullName.Replace("'", ""); }

//                if (!string.IsNullOrEmpty(PhotoDocumentId))
//                { existingDealer.DealerPhotoId = PhotoDocumentId.Replace("'", ""); }


//        string updatedJsonString = JsonSerializer.Serialize(existingDealer);
//        using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(updatedJsonString)))
//        {
//            await _blobService.UploadBlobAsync(jsonFileName, ms, "dealer");
//        }

//        return Ok(new { Message = "Dealer data updated successfully", JsonFile = jsonFileName });
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, new { Message = "An error occurred while updating customer data.", Error = ex.Message });
//    }
//}
//    }
//}





////using Microsoft.AspNetCore.Mvc;
////using Microsoft.Data.SqlClient;
////using OtpAuthServices.Model;
////using System.Data;
////using System.Threading.Tasks;

////[ApiController]
////[Route("api/[controller]")]
////public class DealerController : ControllerBase
////{
////    private readonly string _connectionString;

////    public DealerController(IConfiguration configuration)
////    {
////        _connectionString = configuration.GetConnectionString("DefaultConnection");
////    }

////    [HttpPost("insert-or-update")]
////    public async Task<IActionResult> InsertOrUpdateDealer([FromBody] Dealer dealer)
////    {
////        if (dealer == null)
////            return BadRequest("Invalid dealer request.");

////        string message;

////        try
////        {
////            using (SqlConnection conn = new SqlConnection(_connectionString))
////            {
////                using (SqlCommand cmd = new SqlCommand("Usp_InsertOrUpdateDealer", conn))
////                {
////                    cmd.CommandType = CommandType.StoredProcedure;

////                    // Input Parameters
////                    cmd.Parameters.AddWithValue("@DealerFirmName", dealer.DealerFirmName);
////                    cmd.Parameters.AddWithValue("@DealerFirmRegistrationNumber", dealer.FirmRegistrationNumber);  // This must be passed
////                    cmd.Parameters.AddWithValue("@OwnershipName", dealer.OwnershipName);
////                    cmd.Parameters.AddWithValue("@Category", dealer.Category ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@AadharNumber", dealer.AadharNumber);
////                    cmd.Parameters.AddWithValue("@GSTNumber", dealer.GSTNumber ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@GSTNumberCopy", dealer.GSTNumberCopy ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@PANNumber", dealer.PANNumber);
////                    cmd.Parameters.AddWithValue("@Address", dealer.Address);
////                    cmd.Parameters.AddWithValue("@State", dealer.State);
////                    cmd.Parameters.AddWithValue("@District", dealer.District);
////                    cmd.Parameters.AddWithValue("@ZipCode", dealer.ZipCode);
////                    cmd.Parameters.AddWithValue("@LandMark", dealer.LandMark ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@PhoneNumber", dealer.PhoneNumber);
////                    cmd.Parameters.AddWithValue("@PhoneVerificationCode", dealer.PhoneVerificationCode ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@EmailAddress", dealer.EmailAddress);
////                    cmd.Parameters.AddWithValue("@EmailVerificationCode", dealer.EmailVerificationCode ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@AlternativeMobile", dealer.AlternativeMobile ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@UserId", dealer.UserId == Guid.Empty ? (object)DBNull.Value : dealer.UserId);

////                    // Output Parameter
////                    SqlParameter messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 600)
////                    {
////                        Direction = ParameterDirection.Output
////                    };
////                    cmd.Parameters.Add(messageParam);

////                    // Open connection and execute the stored procedure
////                    await conn.OpenAsync();
////                    await cmd.ExecuteNonQueryAsync();

////                    // Retrieve the output message
////                    message = messageParam.Value.ToString();
////                }
////            }

////            return Ok(new { Message = message });
////        }
////        catch (SqlException ex)
////        {
////            return StatusCode(500, $"Database error: {ex.Message}");
////        }
////        catch (Exception ex)
////        {
////            return StatusCode(500, $"Internal server error: {ex.Message}");
////        }
////    }
////}