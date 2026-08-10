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
    public class RaiseAQuoteController : ControllerBase
    {
        private readonly ICosmosDbService<RaiseAQuote> _cosmosDbService;


        public RaiseAQuoteController(ICosmosDbService<RaiseAQuote> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }


        [HttpPost("CreateRaiseAQuote")]
        public async Task<IActionResult> CreateRaiseAQuote([FromBody] RaiseAQuote raiseAQuote)
        {
            if (raiseAQuote == null)
            {
                return BadRequest("Raise A Quote data cannot be null.");
            }

            raiseAQuote.RaiseAQuoteId = Guid.NewGuid().ToString();
            raiseAQuote.id = Guid.NewGuid().ToString();
            raiseAQuote.QuotedDate = DateTime.UtcNow;
            await _cosmosDbService.AddItemAsync(raiseAQuote);
            return Ok(new { Message = "Raise A Quote created successfully", RaiseAQuoteId = raiseAQuote.id });
        }


        [HttpGet("GetRaiseAQuoteDetails")]
        public async Task<IActionResult> GetRaiseAQuoteDetails()
        {
            try
            {
                var RaiseTicket = await _cosmosDbService.GetRaiseAQuoteDetails();

                // Return 200 OK with tickets
                return Ok(RaiseTicket);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }

        [HttpGet("GetRaiseAQuoteDetailsByid")]
        public async Task<IActionResult> GetRaiseAQuoteDetailsByid(string raiseAQuotetId)
        {
            try
            {
                var RaiseTicket = await _cosmosDbService.GetRaiseAQuoteDetailsById(raiseAQuotetId);


                return Ok(RaiseTicket);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }


        [HttpGet("GetRaiseAQuoteDetailsByTechnicianId")]
        public async Task<IActionResult> GetRaiseAQuoteDetailsByid(string raiseTicketId, string TechnicianId)
        {
            try
            {
                var RaiseTicket = await _cosmosDbService.GetRaiseAQuoteDetailsByTechnicianId( raiseTicketId,  TechnicianId);
                //if (RaiseTicket.materialQuotation.Count == 0)
                //{
                //    MaterialQuotation quotation = new MaterialQuotation();
                //    quotation.grandtotal = "0";
                //    quotation.deliverycharges = "0";
                //    quotation.gst = "0";
                //    quotation.grandtotal = "0";
                //    quotation.servicecharges = "0";
                //    RaiseTicket.materialQuotation.Add(quotation);

                //}

                return Ok(RaiseTicket);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }


        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateRaiseAQuote(string id, [FromBody] RaiseAQuote raiseAQuote)
        {
            if (raiseAQuote == null || raiseAQuote.id != id)
            {
                return BadRequest("RaiseAQuoted information not correct");
            }
            var existRaiseAQuote = await _cosmosDbService.GetItemsAsync(id);
            if (existRaiseAQuote == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(raiseAQuote);
            return Ok("RaiseAQuote Updated Successfully");
        }




        [HttpGet("GetRaiseAQuoteDetailsByQuoteId")]
        public async Task<IActionResult> GetRaiseAQuoteDetailsByQuoteId(string id)
        {
            try
            {
                var RaiseAQuoteId = await _cosmosDbService.GetItemAsync(id);


                return Ok(RaiseAQuoteId);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }


        [HttpGet("GetRaiseAQuoteDetailsByTechnicianIdAndRiseTicketId")]
        public async Task<IActionResult> GetRaiseAQuoteDetailsByTechnicianIdAndRiseTicketId(string TicketId, string TechnicianId)

        {
            try
            {
                var RaiseTicket = await _cosmosDbService.GetRaiseAQuoteDetailsByTechnicianIdAndRiseTicketId( TicketId,  TechnicianId);


                return Ok(RaiseTicket);
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