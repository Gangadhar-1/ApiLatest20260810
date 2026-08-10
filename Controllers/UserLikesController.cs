using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System.Runtime.InteropServices;
using Twilio.TwiML.Messaging;

namespace OtpAuthServices.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class UserLikesController : Controller
    {
        private readonly BlobService _blobService;

        private readonly ICosmosDbService<UserLikes> _cosmosDbService;
        private object _container;

        public UserLikesController(BlobService blobService, ICosmosDbService<UserLikes> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("UploadUserLikesController")]

        public async Task<IActionResult> UploadChatBot([FromBody] UserLikes userLikes)

        {
            if (userLikes == null)
            {

                return BadRequest("UserLikes can not be null");

            }
            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                userLikes.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");
                userLikes.id = Guid.NewGuid().ToString();
                userLikes.userLikesId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(userLikes);

                return Ok(new
                {
                    message = "UserLikes uploaded successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ChatBot.");
                return StatusCode(500, "An error occurred while uploading the ChatBot. Please try again later.");
            }
        }

        [HttpPut("UpdateUserLikes")]
        public async Task<IActionResult> UpdateUserLikes(string id, string userId, [FromBody] UserLikes userLikes)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Id and UserId cannot be null");
            }

            // Fetch record by id
            var existingUserlikes = await _cosmosDbService.GetItemAsync(id);

            if (existingUserlikes == null)
            {
                return NotFound($"No UserLikes found with id {id}");
            }

            // Ensure the userId matches
            if (existingUserlikes.userId != userId)
            {
                return BadRequest("The provided UserId does not match the record's UserId");
            }

            // Update properties
            existingUserlikes.userLikesId = userLikes.userLikesId;
            existingUserlikes.Islike = userLikes.Islike;
            existingUserlikes.messageId = userLikes.messageId;
            existingUserlikes.Date = userLikes.Date;

            // Save update
            await _cosmosDbService.UpdateItemAsync(existingUserlikes);

            return Ok(new
            {
                message = $"UserLikes with id {id} updated successfully for user {userId}"
            });
        }

        [HttpGet("GetUserLikes")]
        public async Task<IActionResult> GetUserLikes(string UserId)
        {
            if (UserId == null)
            {
                return BadRequest("UserId Can not be null");
            }

            var userlikes = await _cosmosDbService.GetUserLikesAsync(UserId);

            return Ok(userlikes);

        }
    }
}
