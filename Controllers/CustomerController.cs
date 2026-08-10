//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Cosmos;
//using OtpAuthServices.AzureService;
//using OtpAuthServices.Model;
//using OtpAuthServices.Models;
//using OtpAuthServices.Services;
//using System;
//using System.IO;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Twilio.Rest.Accounts.V1.Credential;

//namespace OtpAuthServices.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class CustomerController : ControllerBase
//    {
//        private readonly BlobService _blobService;
//        private readonly ICosmosDbService<Customer> _cosmosDbService;
//        private readonly DataGeneratorService _dataGeneratorService;

//        // Modify the constructor to accept the interface ICosmosDbService<Customer>
//        public CustomerController(DataGeneratorService dataGeneratorService, BlobService blobService, ICosmosDbService<Customer> cosmosDbService)
//        {
//            _blobService = blobService;
//            _cosmosDbService = cosmosDbService;
//            _dataGeneratorService = dataGeneratorService;
//        }

//        [HttpPost]

//        [Route("CustomerUpload")]
//        public async Task<IActionResult> UploadUserData([FromForm] Customer customer)
//        {
//            if (customer == null)
//            {
//                return BadRequest("Product cannot be null.");
//            }

//            // Ensure the Id is set
//            customer.id = Guid.NewGuid().ToString(); // Assign a new GUID for Id

//            customer.CustomerId = Guid.NewGuid().ToString();
//            customer.CustomerPhotoId = "download (1).jpg";
//            customer.Status = "Pending";
//            await _cosmosDbService.AddItemAsync(customer);
//            return Ok(new { Message = "Customer data Inserted successfully", JsonFile = customer.id });
//        }



//        [HttpGet("getcustomerdata")]
//        public async Task<IActionResult> Getcustomerdata()
//        {
//            var customer = await _cosmosDbService.GetItemsAsync();
//            if (customer == null)
//            {
//                return BadRequest($"does not find ");
//            }
//            return Ok(customer);
//        }

//        [HttpGet("GetAllCustomersDetails")]

//        public async Task<IActionResult> GetAllCustomersDetails()
//        {

//            try
//            {

//                var data = await _cosmosDbService.GetAllCustomersDetails();

//                return Ok(data);
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(ex.Message);
//            }

//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }


//        [HttpGet("customerProfileData")]
//        public async Task<IActionResult> customerProfileData(string profileType, string UserId)
//        {
//            try
//            {

//                ProfileData profileData = new ProfileData();
//                string sanitizedProfileType = profileType.ToLower();



//                if (string.IsNullOrEmpty(UserId))
//                {
//                    return BadRequest("User Id cannot be emply");
//                }


//                var user = await _cosmosDbService.GetUserProflie(UserId, profileType);

//                if (user != null)
//                {




//                    // Extract FirstName and LastName safely
//                    profileData.FullName = user.FirstName + " " + user.LastName;
//                    profileData.MobileNumber = user.MobileNumber;


//                    // Extract Email and MobileNumber safely
//                    profileData.Email = user.EmailAddress;

//                    // string capitalizedProfileType = char.ToUpper(profileType[0]) + profileType.Substring(1);




//                    profileData.PhotoAttachmentId = user.CustomerPhotoId;





//                    profileData.Address = user.Address;
//                    profileData.UserId = UserId;
//                    profileData.UserProfileType = profileType;
//                    profileData.Status = user.Status;







//                    return Ok(profileData);
//                }
//                else
//                {
//                    return NotFound(new { message = "Customer not found" });
//                }
//            }

//            catch (Exception ex)
//            {
//                // Handle exceptions (for example, log the error and return an internal server error)
//                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
//            }
//        }




//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateCustomerDetails(string id, [FromBody] Customer customer)
//        {
//            if (customer == null || customer.id != id)
//            {
//                return BadRequest("Customer information is incorrect.");
//            }

//            var existingaddress = await _cosmosDbService.GetItemAsync(id);
//            if (existingaddress == null)
//            {
//                return NotFound();
//            }

//            await _cosmosDbService.UpdateItemAsync(customer);
//            return Ok("Customer data Updated successfully.");
//        }



//        [HttpGet("GetCustomerDirectory")]
//        public async Task<IActionResult> GetCustomerDirectory(
// string searchQuery = null,
//string State = null,
// string District = null,
//string Zipcode = null
//)
//        {
//            try
//            {
//                var customers = await _cosmosDbService.GetCustomerDirectoryDetails(searchQuery, State, District, Zipcode);

//                if (customers == null || !customers.Any())
//                {
//                    return NotFound("No customers found with the given criteria.");
//                }

//                return Ok(customers);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//                return StatusCode(500, "An error occurred while retrieving customer details.");
//            }
//        }


//        [HttpGet("{customerId:guid}")]
//        public async Task<IActionResult> GetCustomerId(Guid customerId)
//        {
//            try
//            {
//                var results = await _cosmosDbService.GetCustomerByIdAsync<Customer>(customerId);

//                if (results == null || results.Count == 0)
//                    return NotFound(new { message = "Customer not found." });

//                return Ok(results);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
//            }
//        }



//        [HttpGet("GetCustomerDtailsByUserId")]
//        public async Task<IActionResult> GetUsersData(string userId)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(userId))
//                {
//                    return BadRequest("UserId cannot be null or empty.");
//                }

//                var data = await _cosmosDbService.GetCustomerDetailsByIUserId(userId);


//                return Ok(data);
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                // Log exception here
//                return StatusCode(500, "An error occurred while processing the request.");
//            }
//        }



//        //https://otpauthservices20240928024709.azurewebsites.net/api/Customer/CustomerEdit?UserId='0c7197fc-da28-4a15-8a35-56778056f9c8'&FullName='statyanarayana'&PhotoDocumentId='download (1).jpg'
//        //https://localhost:7091/api/Customer/CustomerEdit?UserId=0c7197fc-da28-4a15-8a35-56778056f9c8&FullName=statyanarayana&PhotoDocumentId=download%20%281%29.jpg
//        [HttpPost]
//        [Route("CustomerEdit")]
//        public async Task<IActionResult> EditUserData(string UserId, string FullName = null, string PhotoDocumentId = null)
//        {
//            // Validate incoming data
//            if (UserId == null)
//            {
//                return BadRequest($"Customer information is incorrect or {UserId} mismatch.");
//            }

//            // Fetch existing customer data from Cosmos DB
//            Customer existingCustomer = null;
//            try
//            {
//                // Assuming you are using the Cosmos DB container to fetch the existing customer by id
//                existingCustomer = await _cosmosDbService.GetUserProflie(UserId, "Customer");  // Assuming this is a method to get the customer by id

//                if (existingCustomer == null)
//                {
//                    return NotFound($"Customer with ID {UserId} not found.");
//                }
//            }
//            catch (CosmosException ex)
//            {
//                // Log CosmosException (error accessing Cosmos DB)
//                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
//            }

//            // Now that we have the existing customer, we can update the necessary fields
//            existingCustomer.FirstName = FullName;
//            existingCustomer.LastName = "";
//            // Example field update
//            //  existingCustomer.LastName =LastName ?? existingCustomer.LastName;
//            //existingCustomer.EmailAddress = customer.EmailAddress ?? existingCustomer.EmailAddress;
//            existingCustomer.CustomerPhotoId = PhotoDocumentId;
//            // Update the existing customer in Cosmos DB
//            try
//            {
//                // Use UpsertItemAsync to update or insert the customer data
//                await _cosmosDbService.UpdateItemAsync(existingCustomer);

//                return Ok(existingCustomer);  // Return the updated customer data
//            }
//            catch (Exception ex)
//            {
//                // Log general exception (other errors)
//                return StatusCode(500, "An error occurred while updating customer data.");
//            }
//        }
//    }

//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using OtpAuthServices.Services;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Twilio.Rest.Accounts.V1.Credential;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<Customer> _cosmosDbService;
        private readonly DataGeneratorService _dataGeneratorService;

        // Modify the constructor to accept the interface ICosmosDbService<Customer>
        public CustomerController(DataGeneratorService dataGeneratorService, BlobService blobService, ICosmosDbService<Customer> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
            _dataGeneratorService = dataGeneratorService;
        }

        [HttpPost]

        [Route("CustomerUpload")]
        public async Task<IActionResult> UploadUserData([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Product cannot be null.");
            }

            // Ensure the Id is set
            customer.id = Guid.NewGuid().ToString(); // Assign a new GUID for Id

            customer.CustomerId = Guid.NewGuid().ToString();
            customer.CustomerPhotoId = "download (1).jpg";
            customer.Status = "Pending";

            //customer.Date=DateTime.Now; 

            await _cosmosDbService.AddItemAsync(customer);

            return Ok(new { Message = "Customer data Inserted successfully", JsonFile = customer.id });
        }



        [HttpPost]

        [Route("GuestCustomerUpload")]
        public async Task<IActionResult> UploadGuestUserData([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Product cannot be null.");
            }

            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
   ? "India Standard Time"
   : "Asia/Kolkata";

            TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);


            
            customer.id = Guid.NewGuid().ToString(); 

            customer.CustomerId = Guid.NewGuid().ToString();
            customer.CustomerPhotoId = "download (1).jpg";
            customer.Status = "Pending";

            customer.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");

            await _cosmosDbService.AddItemAsync(customer);

            return Ok(new { Message = "Customer data Inserted successfully", JsonFile = customer.id });
        }



        [HttpGet("getcustomerdata")]
        public async Task<IActionResult> Getcustomerdata()
        {
            var customer = await _cosmosDbService.GetItemsAsync();
            if (customer == null)
            {
                return BadRequest($"does not find ");
            }
            return Ok(customer);
        }

        [HttpGet("GetAllCustomersDetails")]

        public async Task<IActionResult> GetAllCustomersDetails()
        {

            try
            {

                var data = await _cosmosDbService.GetAllCustomersDetails();

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


        //[HttpGet("customerProfileData")]
        //public async Task<IActionResult> customerProfileData(string profileType, string UserId)
        //{
        //    try
        //    {

        //        ProfileData profileData = new ProfileData();
        //        string sanitizedProfileType = profileType.ToLower();



        //        if (string.IsNullOrEmpty(UserId))
        //        {
        //            return BadRequest("User Id cannot be emply");
        //        }


        //        var user = await _cosmosDbService.GetUserProflie(UserId, profileType);

        //        if (user != null)
        //        {




        //            // Extract FirstName and LastName safely
        //            profileData.FullName = user.FirstName + " " + user.LastName;
        //            profileData.MobileNumber = user.MobileNumber;


        //            // Extract Email and MobileNumber safely
        //            profileData.Email = user.EmailAddress;

        //            // string capitalizedProfileType = char.ToUpper(profileType[0]) + profileType.Substring(1);




        //            profileData.PhotoAttachmentId = user.CustomerPhotoId;





        //            profileData.Address = user.Address;
        //            profileData.UserId = UserId;
        //            profileData.UserProfileType = profileType;
        //            profileData.Status = user.Status;
        //            profileData.ZipCode = user.ZipCode;







        //            return Ok(profileData);
        //        }
        //        else
        //        {
        //            return NotFound(new { message = "Customer not found" });
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        // Handle exceptions (for example, log the error and return an internal server error)
        //        return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
        //    }
        //}



        [HttpGet("customerProfileData")]
        public async Task<IActionResult> CustomerProfileData(string profileType, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("UserId cannot be empty.");
                }

                var user = await _cosmosDbService.GetUserProfilesdata(userId, profileType);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Customer not found."
                    });
                }

                ProfileData profileData = new ProfileData
                {
                    FullName = $"{user.FirstName?.Trim()} {user.LastName?.Trim()}".Trim(),
                    MobileNumber = user.MobileNumber,
                    Email = user.EmailAddress,
                    PhotoAttachmentId = user.CustomerPhotoId,
                    Address = user.Address,
                    UserId = userId,
                    UserProfileType = profileType,
                    Status = user.Status,
                    ZipCode = user.ZipCode,

                };

                return Ok(profileData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing the request.",
                    error = ex.Message
                });
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerDetails(string id, [FromBody] Customer customer)
        {
            if (customer == null || customer.id != id)
            {
                return BadRequest("Customer information is incorrect.");
            }

            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(customer);
            return Ok("Customer data Updated successfully.");
        }



        [HttpGet("GetCustomerDirectory")]
        public async Task<IActionResult> GetCustomerDirectory(
 string searchQuery = null,
 string firstname = null,
string State = null,
 string District = null,
string Zipcode = null
)
        {
            try
            {
                var customers = await _cosmosDbService.GetCustomerDirectoryDetails(searchQuery, firstname, State, District, Zipcode);

                if (customers == null || !customers.Any())
                {
                    return NotFound("No customers found with the given criteria.");
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving customer details.");
            }
        }


        [HttpGet("{customerId:guid}")]
        public async Task<IActionResult> GetCustomerId(Guid customerId)
        {
            try
            {
                var results = await _cosmosDbService.GetCustomerByIdAsync<Customer>(customerId);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "Customer not found." });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }



        [HttpGet("GetCustomerDtailsByUserId")]
        public async Task<IActionResult> GetUsersData(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("UserId cannot be null or empty.");
                }

                var data = await _cosmosDbService.GetCustomerDetailsByIUserId(userId);


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



        //https://otpauthservices20240928024709.azurewebsites.net/api/Customer/CustomerEdit?UserId='0c7197fc-da28-4a15-8a35-56778056f9c8'&FullName='statyanarayana'&PhotoDocumentId='download (1).jpg'
        //https://localhost:7091/api/Customer/CustomerEdit?UserId=0c7197fc-da28-4a15-8a35-56778056f9c8&FullName=statyanarayana&PhotoDocumentId=download%20%281%29.jpg
        [HttpPost]
        [Route("CustomerEdit")]
        public async Task<IActionResult> EditUserData(string UserId, string FullName = null, string PhotoDocumentId = null)
        {
            // Validate incoming data
            if (UserId == null)
            {
                return BadRequest($"Customer information is incorrect or {UserId} mismatch.");
            }

            // Fetch existing customer data from Cosmos DB
            Customer existingCustomer = null;
            try
            {
                // Assuming you are using the Cosmos DB container to fetch the existing customer by id
                existingCustomer = await _cosmosDbService.GetUserProflie(UserId, "Customer");  // Assuming this is a method to get the customer by id

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
            existingCustomer.FirstName = FullName;
            existingCustomer.LastName = "";
            // Example field update
            //  existingCustomer.LastName =LastName ?? existingCustomer.LastName;
            //existingCustomer.EmailAddress = customer.EmailAddress ?? existingCustomer.EmailAddress;
            existingCustomer.CustomerPhotoId = PhotoDocumentId;
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

        [HttpGet("ListOfGuestUsers")]

        public async  Task<IActionResult> ListOfGuestUsers ()
        {
            var users= await  _cosmosDbService.ListOfGuestUsers<Customer>();

            if (users != null)
            {
                return Ok(users);   
            }
                 
            return NotFound("Guest User Not Found");

        }


        [HttpGet("GuestUserExistingVerification/{mobileNo}")]
        public async Task<IActionResult> GuestUserExistingVerification(string mobileNo)
        {
            if (string.IsNullOrEmpty(mobileNo))
            {
                return BadRequest("GuestUser MobileNo cannot be null or empty.");
            }

            var guestuser = await _cosmosDbService.GuestUserExistingVerification<Customer>(mobileNo);
            if (guestuser != null)
            {
                return Ok(guestuser);

            }

            return NotFound($"GuestUser with MobileNo {mobileNo} not found.");
        }



        [HttpPost]
        [Route("CustomerAddressEdit")]
        public async Task<IActionResult> CustomerEdit(AddressModel addressModel)
        {
            if (addressModel == null || string.IsNullOrEmpty(addressModel.id))
            {
                return BadRequest("CustomerAddress information is incorrect or ID mismatch.");
            }

            Customer existingCustomer = null;
            try
            {
                existingCustomer = await _cosmosDbService.GetItemAsync(addressModel.id);

                if (existingCustomer == null)
                {
                    return NotFound($"CustomerAddress with ID {addressModel.id} not found.");
                }
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }

            try
            {
                if (addressModel.UserId != null)
                {

                    existingCustomer.FirstName = addressModel.FirstName;
                    existingCustomer.MobileNumber = addressModel.MobileNumber;
                    existingCustomer.Address = addressModel.Address;
                    existingCustomer.ZipCode = addressModel.ZipCode;
                    existingCustomer.State = addressModel.State;
                    existingCustomer.District = addressModel.District;
                    existingCustomer.StateId = addressModel.StateId;
                    existingCustomer.DistrictId = addressModel.DistrictId;
                    existingCustomer.WalletAmount = addressModel.WalletAmount;

                }
                else
                {
                    existingCustomer.Status = "Open";
                }

                await _cosmosDbService.UpdateItemAsync(existingCustomer);
                return Ok(existingCustomer);
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"An error occurred while updating existingCustomer data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while updating existingCustomer data.");
            }
        }
    }
}















