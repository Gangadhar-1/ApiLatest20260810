using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryNoteController : ControllerBase
    {
        private readonly ICosmosDbService<DeliveryNote> _cosmosDbService;

        public DeliveryNoteController(ICosmosDbService<DeliveryNote> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpPost("CreateDeliveryNote")]
        public async Task<IActionResult> CreateTicket([FromBody] DeliveryNote deliveryNote)
        {
            if (deliveryNote == null)
            {
                return BadRequest("DeliveryNote data cannot be null.");
            }

            try
            {

                deliveryNote.id = Guid.NewGuid().ToString();
                deliveryNote.DeliveryNoteId = Guid.NewGuid().ToString();
                //deliveryNote.DeliveryTime = DateTime.UtcNow;


                await _cosmosDbService.AddItemAsync(deliveryNote);
                return Ok(new
                {
                    Message = "DeliveryNote created successfully",
                    deliveryNoteId = deliveryNote.id

                });
            }
            catch (Exception ex)
            {
                // Log the exception here using a logging framework (e.g., Serilog, NLog, etc.)
                return StatusCode(500, new { Message = "An error occurred while creating the deliveryNote.", Details = ex.Message });
            }
        }

        [HttpGet("GetDeliveryNoteByDeliveryNoteId")]
        public async Task<IActionResult> GetDeliveryNoteByDeliveryNoteId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("DeliveryNote ID cannot be null or empty.");
            }

            var DeliveryNote = await _cosmosDbService.GetItemAsync(id);
            if (DeliveryNote == null)
            {
                return NotFound($"DeliveryNote with ID {id} not found.");
            }

            return Ok(DeliveryNote);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRaiseTicket(string id, [FromBody] DeliveryNote deliveryNote)
        {
            if (deliveryNote == null || deliveryNote.id != id)
            {
                return BadRequest("DeliveryNote information is incorrect.");
            }

            var existingdeliveryNote = await _cosmosDbService.GetItemAsync(id);
            if (existingdeliveryNote == null)
            {
                existingdeliveryNote.id = deliveryNote.id;



            }

            await _cosmosDbService.UpdateItemAsync(deliveryNote);
            return Ok($"DeliveryNote Data Updated Successfully. At with respectiveId {id}.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeliveryNote(string id)
        {
            var existingdeliveryNote = await _cosmosDbService.GetItemAsync(id);
            if (existingdeliveryNote == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Successfully  deleted  DeliveryNote  Item. ");
        }





        [HttpGet("GetRaiseTicketForDealer")]

        public async Task<IActionResult> GetRaiseTicketForDealer(string RaiseTicketId)
        {
            try
            {
                var DeliveryNote = await _cosmosDbService.GetRaiseTicketDetailsForTrader(RaiseTicketId)
 ;

                // Return 200 OK with tickets
                return Ok(DeliveryNote);
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
