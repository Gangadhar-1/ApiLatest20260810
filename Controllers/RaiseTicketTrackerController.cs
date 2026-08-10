using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaiseTicketTrackerController : ControllerBase
    {

        private readonly ICosmosDbService<RaiseTicketTracker> _cosmosDbService;

        public RaiseTicketTrackerController(ICosmosDbService<RaiseTicketTracker> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet("GetRaiseTicketCount")]
        public async Task<IActionResult> GetRaiseTicketCount()
        {
            try
            {
                // Call the service method to get ticket counts by status
                var ticketCounts = await _cosmosDbService.GetRaiseTicketCountAsync();

                if (ticketCounts == null)
                {
                    return StatusCode(500, "Error fetching ticket counts.");
                }

                return Ok(ticketCounts); // Return the dictionary as a JSON response
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error response
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }





        [HttpGet("GetRaiseTicketCountByState")]
        public async Task<IActionResult> GetRaiseTicketCountByState(string state)
        {
            try
            {
                // Call the service method to get ticket counts by status
                var ticketCounts = await _cosmosDbService.GetRaiseTicketCountByStateAsync(state);

                if (ticketCounts == null)
                {
                    return StatusCode(500, "Error fetching ticket counts.");
                }

                return Ok(ticketCounts); // Return the dictionary as a JSON response
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error response
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }

    }
}
