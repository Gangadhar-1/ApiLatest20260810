using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using OtpAuthServices.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RaiseAQuoteByDealerController : ControllerBase
    {
        private readonly ICosmosDbService<RaiseAQuoteByDealer> _cosmosDbService;


        public RaiseAQuoteByDealerController(ICosmosDbService<RaiseAQuoteByDealer> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }


        [HttpPost("CreateRaiseAQuoteByDealer")]
        public async Task<IActionResult> CreateRaiseAQuoteBydealer([FromBody] RaiseAQuoteByDealer raiseAQuoteByDealer)
        {
            if (raiseAQuoteByDealer == null)
            {
                return BadRequest("Raise A Quote By Dealer data cannot be null.");
            }

            raiseAQuoteByDealer.RaiseAQuoteByDealerId = Guid.NewGuid().ToString();
            raiseAQuoteByDealer.id = Guid.NewGuid().ToString();
            raiseAQuoteByDealer.RaiseAQuoteDate = DateTime.UtcNow;
            await _cosmosDbService.AddItemAsync(raiseAQuoteByDealer);
            return Ok(new { Message = "Raise A Quote By Dealer created successfully", RaiseAQuoteByDealerId = raiseAQuoteByDealer.id });
        }


        [HttpGet("GetRaiseAQuoteByDealerDetails")]
        public async Task<IActionResult> GetRaiseAQuoteByDealrDetails()
        {
            try
            {
                var raiseAQuoteByDealer = await _cosmosDbService.GetRaiseAQuoteByDealerDetails();

                // Return 200 OK with tickets
                return Ok(raiseAQuoteByDealer);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateRaiseAQuote(string id, [FromBody] RaiseAQuoteByDealer raiseAQuoteByDealer)
        {
            if (raiseAQuoteByDealer == null || raiseAQuoteByDealer.id != id)
            {
                return BadRequest("RaiseAQuoted information not correct");
            }
            var existRaiseAQuote = await _cosmosDbService.GetItemsAsync(id);
            if (existRaiseAQuote == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(raiseAQuoteByDealer);
            return Ok("RaiseAQuote Updated Successfully");
        }

        [HttpGet("GetRaiseAQuoteDealerDetailsByid")]
        public async Task<IActionResult> GetRaiseAQuoteDealerDetailsByid(string raiseTicketId, string dealerId)
        {
            try
            {
                var RaiseTicket = await _cosmosDbService.GetRaiseAQuoteDealerDetailsById( raiseTicketId,  dealerId);


                return Ok(RaiseTicket);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }

        }

       
            [HttpGet("GetRaiseAQuoteLowestDealerByid")]
            public async Task<IActionResult> GetRaiseAQuoteLowestDealerByid(string raiseAQuotetDealerId)
            {
                try
                {
                    if (string.IsNullOrEmpty(raiseAQuotetDealerId))
                    {
                        return BadRequest("RaiseAQuoteDealerId cannot be null or empty.");
                    }

                    var results = await _cosmosDbService.GetRaiseAQuoteLowestDealerByIdAsync(raiseAQuotetDealerId);

                    if (results == null || results.Count == 0)
                    {
                    RaiseAQuoteByDealer raiseAQuoteByDealer = new RaiseAQuoteByDealer();

                        raiseAQuoteByDealer.DealerId = "Customer Care";

                    return Ok(raiseAQuoteByDealer);
                }

                




                    return Ok(results);
                }
                catch (CosmosException ex)
                {
                    // Log and return 500 Internal Server Error
                    Console.WriteLine($"Cosmos DB error: {ex.Message}");
                    return StatusCode(500, $"Cosmos DB error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Log and return 500 Internal Server Error
                    Console.WriteLine($"Internal server error: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

        }
    
    }
