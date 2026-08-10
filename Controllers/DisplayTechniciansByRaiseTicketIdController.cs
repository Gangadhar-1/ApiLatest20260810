using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisplayTechniciansByRaiseTicketIdController : ControllerBase
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _raiseAQuoteContainer;
        private readonly Container _technicianContainer;

        public DisplayTechniciansByRaiseTicketIdController(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            _raiseAQuoteContainer = _cosmosClient.GetContainer("DatabaseName", "RaiseAQuoteContainer");
            _technicianContainer = _cosmosClient.GetContainer("DatabaseName", "TechnicianContainer");
        }

        [HttpGet("GetTechnicianDetailsByRaiseTicketId/{raiseTicketId}")]
        public async Task<IActionResult> GetTechnicianDetailsByRaiseTicketId(string raiseTicketId)
        {
            if (string.IsNullOrEmpty(raiseTicketId))
            {
                return BadRequest("RaiseTicketId cannot be null or empty.");
            }

            try
            {
                // Step 1: Get the TechnicianId from RaiseAQuote
                var queryDefinition = new QueryDefinition("SELECT c.TechnicianId FROM c where c.RaiseAQuoteId !=null  and c.RaiseTicketId=@RaiseTicketId")
                    .WithParameter("@RaiseTicketId", raiseTicketId);

                var technicianId = string.Empty;
                var queryIterator = _raiseAQuoteContainer.GetItemQueryIterator<dynamic>(queryDefinition);
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        technicianId = item.TechnicianId;
                        break; // Assuming RaiseTicketId maps to a single TechnicianId
                    }
                }

                if (string.IsNullOrEmpty(technicianId))
                {
                    return NotFound("No technician found for the provided RaiseTicketId.");
                }

                // Step 2: Get Technician details using TechnicianId
                var technicianQueryDefinition = new QueryDefinition("SELECT c.id, c.PhoneNumber, c.AlternativePhoneNumber FROM c WHERE c.TechnicianId = @TechnicianId")
                    .WithParameter("@TechnicianId", technicianId);

                var technicianDetails = new List<dynamic>();
                var technicianQueryIterator = _technicianContainer.GetItemQueryIterator<dynamic>(technicianQueryDefinition);
                while (technicianQueryIterator.HasMoreResults)
                {
                    var response = await technicianQueryIterator.ReadNextAsync();
                    technicianDetails.AddRange(response);
                }

                if (technicianDetails.Count == 0)
                {
                    return NotFound("Technician details not found for the given TechnicianId.");
                }

                return Ok(technicianDetails);
            }
            catch (CosmosException ex)
            {
                return StatusCode((int)ex.StatusCode, $"CosmosDB Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}