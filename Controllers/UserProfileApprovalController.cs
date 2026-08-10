using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
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
    public class UserProfileApprovalController : ControllerBase
    {
        private readonly ICosmosDbService<UserProfileApproval> _cosmosDbService;


        public UserProfileApprovalController(ICosmosDbService<UserProfileApproval> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }


        [HttpPost("CreateUserProfileApproval")]
        public async Task<IActionResult> CreateUserProfileApproval([FromBody] UserProfileApproval UserProfileApproval)
        {
            if (UserProfileApproval == null)
            {
                return BadRequest("UserProfileApproval data cannot be null.");
            }

            
            UserProfileApproval.id =  Guid.NewGuid().ToString();
            //UserProfileApproval.RequestedDate = DateTime.UtcNow;
            //UserProfileApproval.ApprovedDate=DateTime.UtcNow;
            await _cosmosDbService.AddItemAsync(UserProfileApproval);
            return Ok(new { Message = "UserProfileApproval created successfully", UserProfileApprovalId = UserProfileApproval.id });
        }



        [HttpGet("VerifyUserApproval")]
        public async Task<IActionResult> VerifyUserApproval(string UserId)
        {
            try
            {
                var userApproval = await _cosmosDbService.VerifyUserApproval(UserId);
                UserProfileApproval userProfileApproval;
                if (userApproval.Count == 0)
                {
                    userProfileApproval = new UserProfileApproval { ApprovedBy = "", ApprovedDate = DateTime.Now, Comments = "", id = UserId, Userid = UserId, RequestedBy = "", RequestedDate = DateTime.Now, Status = "Not Approved" };
                    userApproval.Add(userProfileApproval);

                }
                return Ok(userApproval);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }


    }
}
