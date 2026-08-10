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
    public class ChatBotController : Controller
    {
        private readonly BlobService _blobService;

        private readonly ICosmosDbService<ChatBot> _cosmosDbService;
        private object _container;

        public ChatBotController(BlobService blobService, ICosmosDbService<ChatBot> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("UploadChatBot")]

        public async Task<IActionResult> UploadChatBot([FromBody] ChatBot chatbot)

        {
            if (chatbot == null)
            {

                return BadRequest("Chat Bot can not be null");

            }


            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                chatbot.DateTime = indianTime.ToString("yyyy-MM-ddTHH:mm");
                chatbot.id = Guid.NewGuid().ToString();
                chatbot.ChatBotId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(chatbot);

                return Ok(new
                {
                    message = "ChatBot uploaded successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ChatBot.");
                return StatusCode(500, "An error occurred while uploading the ChatBot. Please try again later.");
            }

        }



        [HttpGet("GetChatBotByUserId")]
        public async Task<IActionResult> GetChatBotMessagesByUserId(string UserId)
        {
            if (UserId == null)
            {
                return BadRequest("UserId Can not be null");
            }

            var chatmessages = await _cosmosDbService.GetItemsByUserIdAsync(UserId);

            return Ok(chatmessages);

        }


        [HttpGet("GetChatMessages")]

        public async Task<IActionResult> GetChatMessages()
        {

            var chatbot = await _cosmosDbService.GetChatMessages<ChatBot>();

            if (chatbot == null)
            {

                return NotFound("Chat Messages Not Found");
            }

            return Ok(chatbot);
        }




        [HttpGet("GetChatMessagesByType")]

        public async Task<IActionResult> GetChatMessagesByType(string type)
        {
            if (type == null)
            {

                return BadRequest("Type Cann not be null");
            }


            var chatmessages = await _cosmosDbService.GetChatMessagesByType<ChatBot>(type);

            if (chatmessages == null)
            {
                return NotFound("Chat Messages Not Found");

            }

            return Ok(chatmessages);


        }

        [HttpPut("UpdateChatBot")]
        public async Task<IActionResult> UpdateChatbot(string id, [FromBody] ChatBot chatbot)
        {

            if (chatbot == null)
            {
                return BadRequest("Chatbot information incorrect");

            }

            var existingchatbot = await _cosmosDbService.GetItemAsync(id);
            if (existingchatbot == null)
            {
                return BadRequest("Chatbot id can not be null");
            }

            existingchatbot.id = chatbot.id;
            existingchatbot.UserName = chatbot.UserName;

            existingchatbot.UserId = chatbot.UserId;
            existingchatbot.DateTime = chatbot.DateTime;
            existingchatbot.ChatType = chatbot.ChatType;

            existingchatbot.ChatBotId = chatbot.ChatBotId;
            existingchatbot.NumberOfLikes = chatbot.NumberOfLikes;
            existingchatbot.Message = chatbot.Message;
            existingchatbot.UploadFile = chatbot.UploadFile;

   

            await _cosmosDbService.UpdateItemAsync(existingchatbot);

            return Ok(new
            {
                message = "ChatBot Updated" + chatbot.id + " Successfully",
            });

        }



        [HttpDelete]

        public async Task<IActionResult> DeleteChatMessage(string id)
        {
            
            var ExistingChatMessage = await _cosmosDbService.GetItemAsync(id);

            if (ExistingChatMessage == null)
            {
                return BadRequest("ChatMessage not found");
            }

            await _cosmosDbService.DeleteItemAsync(id);

            return Ok("Item Deleted successfully");


        }
    }
}
