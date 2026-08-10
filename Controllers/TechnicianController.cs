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
    public class TechnicianController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<Technician> _cosmosDbService;

        public TechnicianController(BlobService blobService, ICosmosDbService<Technician> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }

        //[HttpPost]
        //[Route("TechnicianUpload")]
        //public async Task<IActionResult> UploadUserData([FromForm] Technician technician)
        //{




        //    technician.TechnicianId = Guid.NewGuid();
        //    technician .TechnicianPhotoId =  "download (1).jpg";
        //    // Serialize the UserOnBoarding object to JSON
        //    string jsonString = JsonSerializer.Serialize(technician);

        //    // Create a unique JSON file name based on the username
        //    string jsonFileName = $"{technician.UserId.ToString().Replace(" ", "_")}.json";

        //    // Upload the JSON content to Azure Blob Storage
        //    using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
        //    {
        //        await _blobService.UploadBlobAsync(jsonFileName, ms, "technician");
        //    }

        //    return Ok(new { Message = "Technician data uploaded successfully", JsonFile = jsonFileName });
        //}

        [HttpPost]
        [Route("TechnicianUpload")]
        public async Task<IActionResult> UploadUserData([FromForm] Technician technician)
        {
            {
                if (technician == null)
                {
                    return BadRequest("Product cannot be null.");
                }


                // Ensure the Id is set
                technician.id = Guid.NewGuid().ToString(); // Assign a new GUID for Id

                technician.TechnicianId = Guid.NewGuid();
                technician.Status = "Pending";
                technician.IsActive = true;

                technician.Date= DateTime.Now;  
                await _cosmosDbService.AddItemAsync(technician);
                return Ok(new { Message = "Technician data Inserted successfully", JsonFile = technician.id });
            }

        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTechnicianDetails(string id, [FromBody] Technician technician)
        {
            if (technician == null || technician.id != id)
            {
                return BadRequest("Technician information is incorrect.");
            }

            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(technician);
            return Ok("Technician data Updated successfully.");
        }





        [HttpGet("technicianProfileData")]
        public async Task<IActionResult> technicianProfileData(string profileType, string UserId)
        {
            try
            {

                ProfileData profileData = new ProfileData();
                string sanitizedProfileType = profileType.ToLower();



                if (string.IsNullOrEmpty(UserId))
                {
                    return BadRequest("User Id cannot be empty");
                }


                var user = await _cosmosDbService.GetTechnicianProflie(UserId, profileType);

                if (user != null)
                {




                    // Extract FirstName and LastName safely
                    profileData.FullName = user.TechnicianFullName;
                    profileData.MobileNumber = user.PhoneNumber;
                    profileData.ZipCode = user.ZipCode;

                    //profileData.cate= user.Category;

                    // Extract Email and MobileNumber safely
                    profileData.Email = user.EmailAddress;

                    profileData.Category = user.Category;

                    profileData.District = user.District;      



                    // string capitalizedProfileType = char.ToUpper(profileType[0]) + profileType.Substring(1);




                    profileData.PhotoAttachmentId = user.TechnicianPhotoId;





                    profileData.Address = user.Address;
                    profileData.UserId = UserId;
                    profileData.UserProfileType = profileType;

                    profileData.Status = user.Status;
                    profileData.IsActive = user.IsActive;






                    return Ok(profileData);
                }
                else
                {
                    return NotFound(new { message = "Technician not found" });
                }
            }

            catch (Exception ex)
            {
                // Handle exceptions (for example, log the error and return an internal server error)
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }



        [HttpPost]
        [Route("TechnicianEdit")]
        public async Task<IActionResult> EditUserData(string UserId, string FullName = null, string PhotoDocumentId = null)
        {
            // Validate incoming data
            if (UserId == null)
            {
                return BadRequest($"technician information is incorrect or {UserId} mismatch.");
            }

            // Fetch existing customer data from Cosmos DB
            Technician existingCustomer = null;
            try
            {
                // Assuming you are using the Cosmos DB container to fetch the existing customer by id
                existingCustomer = await _cosmosDbService.GetTechnicianProflie(UserId, "technician");  // Assuming this is a method to get the customer by id

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
            existingCustomer.TechnicianFullName = FullName;
            // existingCustomer.LastName = "";
            // Example field update
            //  existingCustomer.LastName =LastName ?? existingCustomer.LastName;
            //existingCustomer.EmailAddress = customer.EmailAddress ?? existingCustomer.EmailAddress;
            existingCustomer.TechnicianPhotoId = PhotoDocumentId;
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

        [HttpGet("GetAllTechniciansDetails")]



        public async Task<IActionResult> GetAllTechniciansDetails()
        {
            try
            {
                var data = await _cosmosDbService.GetAllTechniciansDetails();


                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }



        [HttpGet("GetTechnicianDetailsByUserId")]
        public async Task<IActionResult> GetTechnicianDetails(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("UserId cannot be null or empty.");
                }

                var data = await _cosmosDbService.GetTechnicianDetailsByUserId(userId);


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







        [HttpGet("TechnicianDirectoryDetails")]
        public async Task<IActionResult> TechnicianDirectoryDetails(string searchQuery = null, string State = null, string District = null, string ZipCode = null, string Status = null)
        {
            try
            {
                Console.WriteLine($"Inputs - SearchQuery: {searchQuery}, State: {State}, District: {District}, ZipCode:{ZipCode} , Status:{Status}");
                var data = await _cosmosDbService.GetTechnicianDirectoryDetails(searchQuery, State, District, ZipCode, Status);

                if (data == null || !data.Any())
                {
                    //    return NotFound("No data found.");
                    return Ok(new { message = "Data not found." });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred.");
            }
        }






        [HttpPut("Status/{TechnicianId}")]
        public async Task<IActionResult> UpdateTechnicianStatus(Guid TechnicianId, [FromQuery] string Status)
        {
            // Fetch the technician details by ID from the database
            var TechnicianList = await _cosmosDbService.GetTechnicianByIdAsync<Technician>(TechnicianId);

            if (TechnicianList == null || TechnicianList.Count == 0)
            {
                return NotFound(new { message = "Technician not found.", isSuccess = false });
            }

            // Assuming only one technician should be returned, take the first one
            var technician = TechnicianList.FirstOrDefault();

            if (technician == null)
            {
                return NotFound(new { message = "Technician not found.", isSuccess = false });
            }

            // Exit early if the status is already the same
            if ((string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase) && technician.Status == "Approved") ||
                (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase) && technician.Status == "Rejected") ||
                (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase) && technician.Status == "Pending"))
            {
                return Ok(new { message = "No changes made. Status is already the same.", isSuccess = true });
            }

            // Update the technician's status based on the Status parameter
            if (string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                technician.IsApproved = true;
                technician.IsRejected = false;
                technician.IsPending = false;
                technician.Status = "Approved"; // Update the Status field
            }
            else if (string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                technician.IsApproved = false;
                technician.IsRejected = true;
                technician.IsPending = false;
                technician.Status = "Rejected"; // Update the Status field
            }
            else if (string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                technician.IsPending = true;
                technician.IsRejected = false;
                technician.IsApproved = false;
                technician.Status = "Pending"; // Update the Status field
            }
            else
            {
                return BadRequest(new { message = "Invalid status. Please use 'Approved', 'Rejected', or 'Pending'.", isSuccess = false });
            }

            // Save the updated technician back to the Cosmos DB
            await _cosmosDbService.UpdateTechnicianAsync(technician);

            return Ok(new
            {
                message = $"Technician status updated to {technician.Status}.",
                isSuccess = true
            });
        }






        [HttpGet("{technicianId:guid}")]
        public async Task<IActionResult> GetTechnicianById(Guid technicianId)
        {
            try
            {
                var results = await _cosmosDbService.GetTechnicianByIdAsync<Technician>(technicianId);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "Technician not found." });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }


        [HttpGet("GetTechnicianDetailsForInvoice")]
        public async Task<ActionResult> GetTechnicianDetailsForInvoice(string TechnicianId)
        {
            try
            {

                // Fetch the total count from the service
                var technicianInvoice = await _cosmosDbService.GetTechnicianDetailsForInvoice(TechnicianId);

                if (technicianInvoice == null)
                {
                    return StatusCode(500, "Error fetching total counts .");
                }
                return Ok(technicianInvoice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected  error occurred.");
            }
        }




        [HttpGet("GetTechnicianMobileAndEmail")]
        public async Task<ActionResult> GetTechnicianMobileAndEmail(string Category, string District)
        {
            try
            {
                var technicianMobileAndEmails = await _cosmosDbService.GetTechnicianMobileAndEmail(Category, District);

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

        [HttpGet("GetTechnicianPincodesBycategory")]
        public async Task<IActionResult> GetTechnicianPincodesBycategory(string Category)
        {
            try
            {
                if (string.IsNullOrEmpty(Category))
                {
                    throw new ArgumentException(nameof(Category), "Category cannot be null or Empty.");
                }

                var techniciapincode = await _cosmosDbService.GetTechnicianPincodesByCategory(Category);
                if (techniciapincode == null || !techniciapincode.Any())
                {
                    return NotFound("No Pincode Found");
                }
                return Ok(techniciapincode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:{ex.Message}");
                return StatusCode(500, "Unexpected error occured.");
            }
        }


        [HttpGet("GetTechniciannamesByPincodeAndCategory")]

        public async Task<IActionResult> GetTechniciannamesByPincode(string pincode, string category)
        {
            try
            {
                if (string.IsNullOrEmpty(pincode))
                {
                    throw new ArgumentException(nameof(pincode), "pincode cannot be null or Empty.");
                }

                var techniciapincode = await _cosmosDbService.GetTechniciannamesByPincode(pincode, category);
                if (techniciapincode == null || !techniciapincode.Any())
                {
                    return NotFound("No TechnicianName  Found");
                }
                return Ok(techniciapincode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:{ex.Message}");
                return StatusCode(500, "Unexpected error occured.");
            }

        }

        [HttpPut("UpdateIsActive/{technicianId}")]
        public async Task<IActionResult> UpdateIsActive(Guid technicianId, [FromBody] bool isActive)
        {
            if (technicianId == Guid.Empty)
            {
                return BadRequest(false); // Return false for invalid technician ID
            }

            // Fetch technician by ID
            var technicians = await _cosmosDbService.GetTechnicianByIdAsync<Technician>(technicianId);

            if (technicians == null || !technicians.Any())
            {
                return NotFound(false); // Return false if technician not found
            }

            var technician = technicians.First();

            if (technician.IsActive == isActive)
            {
                return Ok(false); // Return true if no update is needed
            }

            // Update the IsActive status
            technician.IsActive = isActive;

            // Call the update method and check for success
            var updateSuccess = await _cosmosDbService.UpdateTechnicianAsync(technician);

            if (updateSuccess)
            {
                return Ok(true); // Return true if update was successful
            }

            // If update fails, return false
            return Ok(false);
        }



        //[HttpPut("UpdateIsActive/{techncianId}")]
        //public async Task<IActionResult> UpdateIsActive(Guid techncianId, [FromBody] bool isActive)
        //{
        //    // Fetch the dealer by ID using the GetDealerByIdAsync<T> method
        //    var techncians = await _cosmosDbService.GetTechnicianByIdAsync<Technician>(techncianId);

        //    if (techncians == null || techncians.Count == 0)
        //    {
        //        // Return 404 if dealer not found, with isActive as false
        //        return NotFound(new { message = "technician not found.", isActive = false });
        //    }

        //    // Get the first dealer (assuming only one matches the ID)
        //    var technician = techncians.FirstOrDefault();

        //    if (technician == null)
        //    {
        //        return NotFound(new { message = "technician not found.", isActive = false });
        //    }

        //    // Check if the current IsActive status matches the requested status
        //    if (technician.IsActive == isActive)
        //    {
        //        // Return OK if no update is necessary
        //        return Ok(new
        //        {
        //            message = $"technician is already {(isActive ? "Active" : "Inactive")}.",
        //            isActive = technician.IsActive
        //        });
        //    }

        //    // Update the IsActive property
        //    technician.IsActive = isActive;

        //    // Save the updated dealer back to Cosmos DB
        //    await _cosmosDbService.UpdateTechnicianAsync(technician);

        //    // Return success response
        //    return Ok(new
        //    {
        //        message = $"Technician status successfully updated to {(isActive ? "Active" : "Inactive")}.",
        //        isActive = technician.IsActive
        //    });
        //}
    }
}










//        [HttpPost]
//        [Route("TechnicianEdit")]
//        public async Task<IActionResult> EditUserData(string UserId, string FullName=null, string PhotoDocumentId=null)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(UserId))
//                {
//                    return BadRequest(new { Message = "UserId is required." });
//                }

//                // Replace invalid filename characters in UserId
//                string sanitizedUserId = string.Join("_", UserId.Split(Path.GetInvalidFileNameChars()));
//                string jsonFileName = $"{sanitizedUserId}.json";

//                Stream existingDataStream = await _blobService.DownloadBlobAsync(jsonFileName.Replace("'", ""), "technician");

//                if (existingDataStream == null)
//                {
//                    return NotFound(new { Message = $"Technician data with UserId {UserId} not found." });
//                }

//                string existingData;
//                using (StreamReader reader = new StreamReader(existingDataStream))
//                {
//                    existingData = await reader.ReadToEndAsync();
//                }

//                Technician existingTechnician;
//                try
//                {
//                    existingTechnician = JsonSerializer.Deserialize<Technician>(existingData);
//                }
//                catch (JsonException jsonEx)
//                {
//                    return StatusCode(500, new { Message = "Failed to parse customer data.", Error = jsonEx.Message });
//                }
//                if(!string.IsNullOrEmpty(FullName)) 
//                    existingTechnician.TechnicianFullName = FullName.Replace("'", "");

//                if(!string.IsNullOrEmpty(PhotoDocumentId))  
//                    existingTechnician.TechnicianPhotoId = PhotoDocumentId.Replace("'", ""); 

//                string updatedJsonString = JsonSerializer.Serialize(existingTechnician);
//                using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(updatedJsonString)))
//                {
//                    await _blobService.UploadBlobAsync(jsonFileName, ms, "technician");
//                }

//                return Ok(new { Message = "Technician data updated successfully", JsonFile = jsonFileName });
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
//using System.IO;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using OtpAuthServices.Model;

//namespace OtpAuthServices.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class technicianController : ControllerBase
//    {
//        private readonly IConfiguration _configuration;
//        private readonly string _connectionString;


//        public technicianController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//            _connectionString = _configuration.GetConnectionString("DefaultConnection");
//        }

//        [HttpPost("InsertOrUpdatetechnician")]
//        public async Task<IActionResult> InsertOrUpdatetechnician([FromBody] Technician technician)
//        {
//            if (technician == null)
//            {
//                return BadRequest("Technician data is null");
//            }

//            try
//            {

//                using (SqlConnection conn = new SqlConnection(_connectionString))
//                {
//                    await conn.OpenAsync();

//                    using (SqlCommand cmd = new SqlCommand("Usp_InsertOrUpdatetechnician", conn))
//                    {
//                        cmd.CommandType = CommandType.StoredProcedure;

//                        cmd.Parameters.AddWithValue("@firstname", technician.FirstName);
//                        cmd.Parameters.AddWithValue("@LastName", technician.LastName);
//                        cmd.Parameters.AddWithValue("@GSTNumber", technician.GSTNumber);
//                        cmd.Parameters.AddWithValue("@PANNumber", technician.PANNumber);
//                        cmd.Parameters.AddWithValue("@PanCard", technician.PanCard);
//                        cmd.Parameters.AddWithValue("@AadharNumber", technician.AadharNumber);
//                        cmd.Parameters.AddWithValue("@AaadharCardPhoto", technician.AaadharCardPhoto);
//                        cmd.Parameters.AddWithValue("@Address", technician.Address);
//                        cmd.Parameters.AddWithValue("@State", technician.State);
//                        cmd.Parameters.AddWithValue("@District", technician.District);
//                        cmd.Parameters.AddWithValue("@ZipCode", technician.ZipCode);

//                        cmd.Parameters.AddWithValue("@PhoneNumber", technician.PhoneNumber);
//                        cmd.Parameters.AddWithValue("@AlternativePhoneNumber", technician.AlternativePhoneNumber);
//                        cmd.Parameters.AddWithValue("@PhoneVerificationCode", technician.PhoneVerificationCode);
//                        cmd.Parameters.AddWithValue("@EmailAddress", technician.EmailAddress);
//                        cmd.Parameters.AddWithValue("@EmailVerificationCode", technician.EmailVerificationCode);
//                        cmd.Parameters.AddWithValue("@technicianPhoto", technician.technicianPhoto);
//                        cmd.Parameters.AddWithValue("@Category", technician.Category);
//                        cmd.Parameters.AddWithValue("@UserId", technician.UserId);

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
//    }


