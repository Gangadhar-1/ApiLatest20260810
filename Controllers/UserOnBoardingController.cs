//using Microsoft.AspNetCore.Mvc;
//using OtpAuthServices.AzureService;
//using OtpAuthServices.Model;
//using System;
//using System.Threading.Tasks;

//namespace OtpAuthServices.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class UserOnBoardingController : ControllerBase
//    {
//        private readonly ICosmosDbService<UserOnBoarding> _cosmosDbService;

//        // Constructor now accepts ICosmosDbService<UserOnBoarding>
//        public UserOnBoardingController(ICosmosDbService<UserOnBoarding> cosmosDbService)
//        {
//            _cosmosDbService = cosmosDbService;
//        }

//        [HttpPost]
//        [Route("UserUpload")]
//        public async Task<IActionResult> UploadUserData([FromForm] UserOnBoarding userOnBoarding)
//        {
//            // Assign a new GUID to the UserId
//            userOnBoarding.UserId = Guid.NewGuid(); 

//            userOnBoarding.id = Guid.NewGuid().ToString();

//            // Insert the UserOnBoarding object into Cosmos DB
//            await _cosmosDbService.AddItemAsync(userOnBoarding);


//            return Ok(new { Message = "User data uploaded successfully", UserId = userOnBoarding.UserId });
//        }





//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateUserOnBoarding(string id, [FromBody] UserOnBoarding userOnBoarding)
//        {
//            if (userOnBoarding == null || userOnBoarding.id != id)
//            {
//                return BadRequest("Product information is incorrect.");
//            }

//            var existingProduct = await _cosmosDbService.GetItemAsync(id);
//            if (existingProduct == null)
//            {
//                existingProduct.id = userOnBoarding.id;


//            }
//            existingProduct.id.Replace("/", string.Empty);
//            await _cosmosDbService.UpdateItemAsync(userOnBoarding);
//            return Ok($"RaiseTicket Data Updated Successfully. At with respectiveId {id}.");
//}

//         [HttpGet]   
//        [Route("VerifyUserProfile")]
//        public async Task<IActionResult> GetUser(string value)
//        {
//            if (string.IsNullOrEmpty(value))
//            {
//                return BadRequest("Either mobile number or email address must be provided.");
//            }

//            // Use the GetUserByEmailOrMobileAsync method to fetch the user
//            var user = await _cosmosDbService.GetUserByEmailOrMobileAsync(value);

//            if (user != null)
//            {
//                return Ok(user);
//            }

//            return NotFound("User not found.");
//        }

//            [HttpGet("VerifyUserLogin")]
//        public async Task<IActionResult> VerifyUserLogin(string username, string password)
//        {
//            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
//            {
//                return BadRequest("Either mobile number or email address must be provided.");
//            }

//            // Use the GetUserByEmailOrMobileAsync method to fetch the user
//            var user = await _cosmosDbService.GetUserByLogin(username, password);

//            if (user != null)
//            {
//                return Ok(user);
//            }

//            return NotFound("User not found.");
//        }


using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserOnBoardingController : ControllerBase
    {
        private readonly ICosmosDbService<UserOnBoarding> _cosmosDbService;

        // Constructor now accepts ICosmosDbService<UserOnBoarding>
        public UserOnBoardingController(ICosmosDbService<UserOnBoarding> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost]
        [Route("UserUpload")]
        public async Task<IActionResult> UploadUserData([FromBody] UserOnBoarding userOnBoarding)
        {
            userOnBoarding.UserId = Guid.NewGuid();

            userOnBoarding.id = Guid.NewGuid().ToString();

            //userOnBoarding.Date= DateTime.Now;  

            await _cosmosDbService.AddItemAsync(userOnBoarding);


            return Ok(new { Message = "User data uploaded successfully", UserId = userOnBoarding.UserId });
        }

        [HttpPost]
        [Route("GuestUserUpload")]
        public async Task<IActionResult> GuestUploadUserData([FromBody] UserOnBoarding userOnBoarding)
        {
            userOnBoarding.UserId = Guid.NewGuid();

            userOnBoarding.id = Guid.NewGuid().ToString();


            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
  ? "India Standard Time"
  : "Asia/Kolkata";

            TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);


            userOnBoarding.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");

            await _cosmosDbService.AddItemAsync(userOnBoarding);


            return Ok(new { Message = "User data uploaded successfully", UserId = userOnBoarding.UserId });
        }


        [HttpGet]
        [Route("GuestUserVerificationByMobileNo")]
        public async Task<IActionResult> GuestUserVerificationByMobileNo(string mobileNo)
        {
            if (string.IsNullOrEmpty(mobileNo))
            {
                return BadRequest("mobile number Not found.");
            }

            var user = await _cosmosDbService.GuestUserVerificationByMobileNo(mobileNo);

            if (user != null)
            {
                return Ok(user);
            }

            return NoContent(); // ✅ correct
        }

        private IActionResult NoContent(string v)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserOnBoarding(string id, [FromBody] UserOnBoarding userOnBoarding)
        {
            if (userOnBoarding == null || userOnBoarding.id != id)
            {
                return BadRequest("Product information is incorrect.");
            }

            var existingProduct = await _cosmosDbService.GetItemAsync(id);
            if (existingProduct == null)
            {
                existingProduct.id = userOnBoarding.id;


            }
            existingProduct.id.Replace("/", string.Empty);
            await _cosmosDbService.UpdateItemAsync(userOnBoarding);
            return Ok($"RaiseTicket Data Updated Successfully. At with respectiveId {id}.");




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
            var user = await _cosmosDbService.GetUserByLogin(username, password);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("User not found.");
        }
}
}


    

