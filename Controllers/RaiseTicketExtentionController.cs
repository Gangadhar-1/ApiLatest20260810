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
    public class RaiseTicketExtentionController : ControllerBase
    {
        private readonly ICosmosDbService<RaiseTicketExtention> _cosmosDbService;

        // Constructor to initialize dependencies
        public RaiseTicketExtentionController(ICosmosDbService<RaiseTicketExtention> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }
        
    
        [HttpPost("CreateRaiseTicketExtension")]
        public async Task<IActionResult> CreateTicket([FromBody] RaiseTicketExtention RaiseTicketExtention)
        {
            if (RaiseTicketExtention == null)
            {
                return BadRequest("Ticket data cannot be null.");
            }

           string ticketId = GenerateRaiseTicketIds();


            RaiseTicketExtention.id= Guid.NewGuid().ToString();
            //RaiseTicket.status = "Open";
            //RaiseTicket.Date = DateTime.UtcNow;
            RaiseTicketExtention.RaiseTicketIdVideoRef = ticketId;

            // Insert the support ticket into Cosmos DB
            await _cosmosDbService.AddItemAsync(RaiseTicketExtention);
            return Ok(new { Message = "Raise ticket created successfully", RaiseTicketId = RaiseTicketExtention.id, VideoRefId = RaiseTicketExtention.RaiseTicketIdVideoRef });
        }
        private string GenerateRaiseTicketIds()
        {
            Random random = new Random();
            string prefix = "VSKPAPREFV"; // Fixed prefix
            string numbers = random.Next(1000, 9999).ToString(); // Random 4-digit number
            char letter = (char)random.Next('A', 'Z' + 1); // Random uppercase letter

            return $"{prefix}{numbers}{letter}";
        }

    }
}
