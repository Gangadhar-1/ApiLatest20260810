using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using Twilio.TwiML.Messaging;

namespace OtpAuthServices.Controllers
{

    [ApiController]

    [Route("api/[controller]")]

    public class AddToCartController : Controller
    {

        private readonly BlobService _blobService;

        private readonly ICosmosDbService<AddToCart> _cosmosDbService;


        public AddToCartController(BlobService blobService, ICosmosDbService<AddToCart> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("AddToCartUpload")]

        public async Task<IActionResult> AddToCartUpload([FromBody] AddToCart addToCart)
        {
            if (addToCart == null)
            {

                return BadRequest("Add To cart Cannot Be null");
            }

            try
            {
                addToCart.id = Guid.NewGuid().ToString();

                addToCart.AddToCartId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(addToCart);

                return Ok(new
                {
                    Message = "Item Add To Cart Successfully."
                });

            }

            catch (Exception ex)
            {
                {
                    _logger.LogError(ex, "Error Add To Cart");
                    return StatusCode(500, "An error occurred while Add To Cart. Please try again.");
                }

            }
        }

    }
}
