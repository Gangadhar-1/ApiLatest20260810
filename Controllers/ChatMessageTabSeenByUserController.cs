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
    public class ChatMessageTabSeenByUserController : Controller
    {
        private readonly BlobService _blobService;

        private readonly ICosmosDbService<ChatMessageTabSeenByUser> _cosmosDbService;
        private object _container;

        public ChatMessageTabSeenByUserController(BlobService blobService, ICosmosDbService<ChatMessageTabSeenByUser> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("UploadChatMessageTabSeenByUser")]

        public async Task<IActionResult> UploadChatMessageTabSeenByUser([FromBody] ChatMessageTabSeenByUser chatMessageTabSeenByUser)

        {
            if (chatMessageTabSeenByUser == null)
            {

                return BadRequest("ChatMessageTabSeenByUser can not be null");

            }


            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                chatMessageTabSeenByUser.DateTime = indianTime.ToString("yyyy-MM-ddTHH:mm");
                chatMessageTabSeenByUser.id = Guid.NewGuid().ToString();
                chatMessageTabSeenByUser.ChatMessageTabSeenByUserId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(chatMessageTabSeenByUser);

                return Ok(new
                {
                    message = "chatMessageTabSeenByUser uploaded successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ChatBot.");
                return StatusCode(500, "An error occurred while uploading the ChatBot. Please try again later.");
            }
        }




        [HttpGet("GetChatMessageTabSeenByUserById")]

        public async Task<IActionResult> GetChatMessageTabSeenByUser(string id)


        {
            if (id == null)
            {
                return BadRequest("id can not be null");

            }

            var ChatMessageTabSeenByUser = await _cosmosDbService.GetItemAsync(id);

            return Ok(ChatMessageTabSeenByUser);


        }
    

        [HttpPut("UpdateChatMessageTabSeenByUser")]

        public async Task<IActionResult> UpdateChatMessageTabSeenByUser(string id,[FromBody] ChatMessageTabSeenByUser chatMessageTabSeenByUser)

        {
            if (chatMessageTabSeenByUser == null)
            {
                return BadRequest("chatMessageTabSeenByUser Can not be null");
            }


            var existingChatMessageTabSeenByUser = await _cosmosDbService.GetItemAsync(id);

            if (existingChatMessageTabSeenByUser == null)

            {
                return BadRequest("existingChatMessageTabSeenByUser Can not be null");


            }

            existingChatMessageTabSeenByUser.id = chatMessageTabSeenByUser.id;
            existingChatMessageTabSeenByUser.ChatMessageTabSeenByUserId = chatMessageTabSeenByUser.ChatMessageTabSeenByUserId;
            existingChatMessageTabSeenByUser.UserId = chatMessageTabSeenByUser.UserId;
            existingChatMessageTabSeenByUser.ChatTabNews = chatMessageTabSeenByUser.ChatTabNews;

            existingChatMessageTabSeenByUser.ChatTabBuySell = chatMessageTabSeenByUser.ChatTabBuySell;
            existingChatMessageTabSeenByUser.ChatTabTolet = chatMessageTabSeenByUser.ChatTabTolet;


            await _cosmosDbService.UpdateItemAsync(chatMessageTabSeenByUser);

            return Ok(new
            {
                message = "ChatMessageTabSeenByUser Updated" + chatMessageTabSeenByUser.id + " Successfully",
            });

        }
    }
    
}



