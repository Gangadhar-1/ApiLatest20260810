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
    public class ApartmentRaiseTicketController : ControllerBase
    {
        private readonly ICosmosDbService<ApartmentRaiseTicket> _cosmosDbService;


        public ApartmentRaiseTicketController(ICosmosDbService<ApartmentRaiseTicket> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }



        [HttpPost("CreateApartmentRaiseTicket")]
        public async Task<IActionResult> CreateTicket([FromBody] ApartmentRaiseTicket apartmentRaiseTicket)
        {
            if (apartmentRaiseTicket == null)
            {
                return BadRequest("Ticket data cannot be null.");
            }

            string apmcaId = GenerateApartmentMaintenanceTicket();

            apartmentRaiseTicket.id = Guid.NewGuid().ToString();

            apartmentRaiseTicket.ApartmentRaiseTicketId = apmcaId;


            apartmentRaiseTicket.Date = DateTime.UtcNow;

            await _cosmosDbService.AddItemAsync(apartmentRaiseTicket);
            return Ok(new { Message = "ApartmentRaiseTicket created successfully", ApartmentMaintenanceId = apartmentRaiseTicket.id, ApartmentRaiseTicketId = apartmentRaiseTicket.ApartmentRaiseTicketId });
        }


        private string GenerateApartmentMaintenanceTicket()
        {
            Random rnd = new Random();

            string prefix = "APMCA";
            string numbers = rnd.Next(1000, 9999).ToString();

            char letter = (char)rnd.Next('A', 'Z' + 1);

            return $"{prefix}{numbers}{letter}";
        }




        [HttpGet("GetGetApartmentMaintenanceForAdminList")]
        public async Task<IActionResult> GetApartmentMaintenanceForAdminList()
        {


            var apartmentMaintenance = await _cosmosDbService.GetApartmentMaintenanceForAdminList<ApartmentRaiseTicket>();
            if (apartmentMaintenance == null)
            {
                return NotFound($"ApartmentRaiseTicketId   not found.");
            }

            return Ok(apartmentMaintenance);
        }


        [HttpGet("GetApartmentMaintenanceRaiseTicket/{id}")]
        public async Task<IActionResult> GetApartmentMaintenanceRaiseTicket(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Ticket ID cannot be null or empty.");
            }

            var apartmentMaintenanceRaiseTicket = await _cosmosDbService.GetItemAsync(id);
            if (apartmentMaintenanceRaiseTicket == null)
            {
                return NotFound($"ApartmentMaintenanceRaiseTicket with ID {id} not found.");
            }

            return Ok(apartmentMaintenanceRaiseTicket);
        }


    }

}