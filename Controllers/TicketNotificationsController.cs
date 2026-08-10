using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketNotificationsController : ControllerBase
    {
        private readonly ICosmosDbService<TicketNotifications> _cosmosDbService;

        // Constructor to initialize dependencies
        public TicketNotificationsController(ICosmosDbService<TicketNotifications> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }



        [HttpPost("CreateTicketNotifications")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketNotifications ticketNotifications)
        {
            if (ticketNotifications == null)
            {
                return BadRequest("TicketNotifications data cannot be null.");
            }

            ticketNotifications.id = Guid.NewGuid().ToString();


            await _cosmosDbService.AddItemAsync(ticketNotifications);
            return Ok(new { Message = "TicketNotifications created successfully", TicketNotificationsId = ticketNotifications.id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketNotifications(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Successfully  deleted  TicketNotifications  Item. ");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicketNotifications(string id, [FromBody] TicketNotifications ticketNotifications)
        {
            if (ticketNotifications == null || ticketNotifications.id != id)
            {
                return BadRequest("TicketNotifications information is incorrect.");
            }

            var existingTicketNotifications = await _cosmosDbService.GetItemAsync(id);
            if (existingTicketNotifications == null)
            {
                existingTicketNotifications.TicketNotificationId = ticketNotifications.TicketNotificationId;



            }

            await _cosmosDbService.UpdateItemAsync(ticketNotifications);
            return Ok($"TicketNotifications Data Updated Successfully. At  respectiveId {id}.");




        }

        [HttpGet("GetTicketNotificationsById")]
        public async Task<IActionResult> GetTicketNotificationsById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("TicketNotifications ID cannot be null or empty.");
            }

            var TicketNotifications = await _cosmosDbService.GetItemAsync(id);
            if (TicketNotifications == null)
            {
                return NotFound($"TicketNotifications with ID {id} not found.");
            }

            return Ok(TicketNotifications);
        }


    }
}