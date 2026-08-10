using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;

namespace OtpAuthServices.Controllers
{

    [ApiController]

    [Route("api/[controller]")]
    public class MarkMessageSeenController : Controller
    {
        private readonly BlobService _blobService;

        private readonly ICosmosDbService<MarkMessageSeen> _cosmosDbService;

        public MarkMessageSeenController(BlobService blobService, ICosmosDbService<MarkMessageSeen> cosmosDbService)


        {
            _blobService = blobService;

            _cosmosDbService = cosmosDbService;
        }

        [HttpPost("UploadMessageSeenCount")]

        public async Task<IActionResult> UploadMessageSeenCount([FromBody] MarkMessageSeen markmessageseen)

        {
            if (markmessageseen == null)
            {

                return BadRequest("markmessageseen  can not be null");
             }
            try
            {
                markmessageseen.id = Guid.NewGuid().ToString();


                await _cosmosDbService.AddItemAsync(markmessageseen);
                return Ok(new
                {
                    message = "Markmessageseen uploaded successfully.  MarkMessageSeen Id Is" + markmessageseen.id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ChatBot.");
                return StatusCode(500, "An error occurred while uploading the ChatBot. Please try again later.");
            }

        }



        [HttpGet]
        [Route("GetMarkMessageSeenUsers")]
        public async Task<IActionResult> GetMarkMessageSeenUsers(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
                return BadRequest("messageId not found.");

            var messageSeen = await _cosmosDbService.GetMarkMessageSeen(messageId);

            if (messageSeen != null)
                return Ok(new { UsersCount = messageSeen.UsersCount }); 

            return NotFound("messageSeen not found.");
        }


        public class MessageSeenCount
        {
            public string messageId { get; set; }
            public int userCount { get; set; }
            public int UsersCount { get; internal set; }
            public int UsersCountss { get; internal set; }
        }

    }
}

