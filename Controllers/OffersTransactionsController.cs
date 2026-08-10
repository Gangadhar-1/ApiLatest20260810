using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System.Runtime.InteropServices;

namespace OtpAuthServices.Controllers
{

    [ApiController]

    [Route("api/[controller]")]


    public class OffersTransactionsController : Controller
    {
        private readonly ICosmosDbService<OffersTransactions> _cosmosDbService;
        private readonly ILogger<OffersTransactions> _logger;

        public OffersTransactionsController(ICosmosDbService<OffersTransactions> cosmosDbService, ILogger<OffersTransactions> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadOffersTransactionsDetails")]
        public async Task<IActionResult> UploadOffersTransactionsDetails([FromBody] OffersTransactions offersTransactions)
        {
            if (offersTransactions == null)
            {
                return BadRequest("offersTransactions cannot be null");
            }

            try
            {
                offersTransactions.id = Guid.NewGuid().ToString();


                await _cosmosDbService.AddItemAsync(offersTransactions);

                return Ok(new
                {
                    message = "offersTransactions details uploaded successfully.",
                    id = offersTransactions.id,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading offersTransactions Details.");
                return StatusCode(500, "An error occurred while uploading the offersTransactions Details. Please try again later.");
            }
        }



        [HttpGet("GetOfferTransactionByUserId")]
        public async Task<IActionResult> GetOfferTransactionByUserId(string userId)
        {
            var offerTransaction = await _cosmosDbService.GetOfferTransactionByUserId(userId);


            if (offerTransaction == null)
            {

                return BadRequest("GetOfferTransaction  not found");
            }

            return Ok(offerTransaction);
        }






        [HttpPut("UpdateOffersTransactionsDetails/{id}")]
        public async Task<IActionResult> UpdateBanner(string id, [FromBody] OffersTransactions offersTransactions)
        {

            if (offersTransactions == null || offersTransactions.id != id)
            {
                return BadRequest("Upload OffersTransactions information incorrect");

            }

            try
            {

                var existingOffersTransactions = await _cosmosDbService.GetItemAsync(id);

                existingOffersTransactions.UpdatedDate = offersTransactions.UpdatedDate;
                existingOffersTransactions.AvailedAmount = offersTransactions.AvailedAmount;
                existingOffersTransactions.TotalWalletAmount = offersTransactions.TotalWalletAmount;
                existingOffersTransactions.RemainingAmount = offersTransactions.RemainingAmount;
                /// existingOffersTransactions.TicketId = offersTransactions.TicketId;






                await _cosmosDbService.UpdateItemAsync(offersTransactions);
                return Ok(new
                {
                    message = "offersTransactions details updated successfully.",
                    id = existingOffersTransactions.id,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating offersTransactions record with id {id}");
                return StatusCode(500, "An error occurred while updating the offersTransactions details. Please try again later.");
            }
        }

    }
}
