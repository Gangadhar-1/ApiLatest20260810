
//﻿using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Cosmos;
//using OtpAuthServices.AzureService;
//using OtpAuthServices.Model;
//using OtpAuthServices.Models;
//using OtpAuthServices.Services;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;


//namespace OtpAuthServices.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class GuestUserController : ControllerBase
//    {
//        private readonly ICosmosDbService<GuestUser> _cosmosDbService;
//        public GuestUserController(ICosmosDbService<GuestUser> cosmosDbService)
//        {
//            _cosmosDbService = cosmosDbService;

//        }

//        [HttpPost("UploadGuestUser")]
//        public async Task<IActionResult> UploadGuestUser([FromBody] GuestUser GuestUser)
//        {
//            if (GuestUser == null)
//            {
//                return BadRequest("GuestUser data cannot be null.");
//            }
//            GuestUser.UserId = Guid.NewGuid().ToString();

//            GuestUser.DateTime = DateTime.UtcNow;
//            GuestUser.id = Guid.NewGuid().ToString();

//            await _cosmosDbService.AddItemAsync(GuestUser);
//            return Ok(new { Message = "GuestUser Data Uploaded successfully", GuestUserId = GuestUser.UserId });
//        }


//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteGuestUser(string id)
//        {
//            var existingaddress = await _cosmosDbService.GetItemAsync(id);
//            if (existingaddress == null)
//            {
//                return NotFound();
//            }

//            await _cosmosDbService.DeleteItemAsync(id);
//            return Ok("Successfully  deleted GuestUser Data   Item. ");
//        }

        
//        [HttpGet("GuestUser/{id}")]
//        public async Task<IActionResult> GetBookTechnician(string id)
//        {
//            if (string.IsNullOrEmpty(id))
//            {
//                return BadRequest("GuestUser ID cannot be null or empty.");
//            }

//            var bookTechnician = await _cosmosDbService.GetItemAsync(id);
//            if (bookTechnician == null)
//            {
//                return NotFound($"GuestUser with ID {id} not found.");
//            }

//            return Ok(bookTechnician);
//        }

      
        
        
//        [HttpGet("GuestUserExistingVerification/{mobileNo}")]
//        public async Task<IActionResult> GuestUserExistingVerification(string mobileNo)
//        {
//            if (string.IsNullOrEmpty(mobileNo))
//            {
//                return BadRequest("GuestUser MobileNo cannot be null or empty.");
//            }

//            var guestuser = await _cosmosDbService.GuestUserExistingVerification<GuestUser>(mobileNo);
//            if (guestuser != null)
//            {
//                return Ok(guestuser);
               
//            }

//            return NotFound($"GuestUser with MobileNo {mobileNo} not found.");
//        }



//        [HttpGet("GetGuestUserProfileData")]
//        public async Task<IActionResult> GetGuestUserProfileData(string profileType, string userId)
//        {
//            if (string.IsNullOrEmpty(profileType) || string.IsNullOrEmpty(userId))
//            {
//                return BadRequest("ProfileType and UserId cannot be null or empty.");
//            }

//            var userProfile = await _cosmosDbService.GetGuestUserProfileData(profileType, userId);

//            if (userProfile == null)
//            {
//                return NotFound("UserProfileData not found.");
//            }

//            return Ok(userProfile);
//        }





//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateGuestUser(string id, [FromBody] GuestUser GuestUser)
//        {
//            if (GuestUser == null || GuestUser.id != id)
//            {
//                return BadRequest("GuestUser information is incorrect.");
//            }

//            var existingaddress = await _cosmosDbService.GetItemAsync(id);
//            if (existingaddress == null)
//            {
//                return NotFound();
//            }

//            await _cosmosDbService.UpdateItemAsync(GuestUser);
//            return Ok("GuestUser data Updated successfully.");
//        }



//    }

﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using OtpAuthServices.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestUserController : ControllerBase
    {
        private readonly ICosmosDbService<GuestUser> _cosmosDbService;
        public GuestUserController(ICosmosDbService<GuestUser> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }

        [HttpPost("UploadGuestUser")]
        public async Task<IActionResult> UploadGuestUser([FromBody] GuestUser GuestUser)
        {
            if (GuestUser == null)
            {
                return BadRequest("GuestUser data cannot be null.");
            }
            GuestUser.UserId = Guid.NewGuid().ToString();

            GuestUser.DateTime = DateTime.UtcNow;
            GuestUser.id = Guid.NewGuid().ToString();

            await _cosmosDbService.AddItemAsync(GuestUser);
            return Ok(new { Message = "GuestUser Data Uploaded successfully", GuestUserId = GuestUser.UserId });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuestUser(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Successfully  deleted GuestUser Data   Item. ");
        }

        
        [HttpGet("GuestUser/{id}")]
        public async Task<IActionResult> GetBookTechnician(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("GuestUser ID cannot be null or empty.");
            }

            var bookTechnician = await _cosmosDbService.GetItemAsync(id);
            if (bookTechnician == null)
            {
                return NotFound($"GuestUser with ID {id} not found.");
            }

            return Ok(bookTechnician);
        }

      
        
        
        [HttpGet("GuestUserExistingVerification/{mobileNo}")]
        public async Task<IActionResult> GuestUserExistingVerification(string mobileNo)
        {
            if (string.IsNullOrEmpty(mobileNo))
            {
                return BadRequest("GuestUser MobileNo cannot be null or empty.");
            }

            var guestuser = await _cosmosDbService.GuestUserExistingVerification<GuestUser>(mobileNo);
            if (guestuser != null)
            {
                return Ok(guestuser);
               
            }

            return NotFound($"GuestUser with MobileNo {mobileNo} not found.");
        }



        [HttpGet("GetGuestUserProfileData")]
        public async Task<IActionResult> GetGuestUserProfileData(string profileType, string userId)
        {
            if (string.IsNullOrEmpty(profileType) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("ProfileType and UserId cannot be null or empty.");
            }

            var userProfile = await _cosmosDbService.GetGuestUserProfileData(profileType, userId);

            if (userProfile == null)
            {
                return NotFound("UserProfileData not found.");
            }

            return Ok(userProfile);
        }





        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuestUser(string id, [FromBody] GuestUser GuestUser)
        {
            if (GuestUser == null || GuestUser.id != id)
            {
                return BadRequest("GuestUser information is incorrect.");
            }

            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(GuestUser);
            return Ok("GuestUser data Updated successfully.");
        }



    }
}