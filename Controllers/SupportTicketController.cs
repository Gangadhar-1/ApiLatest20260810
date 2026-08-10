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
    public class SupportTicketController : ControllerBase
    {
        private readonly ICosmosDbService<SupportTicket> _cosmosDbService;

        // Constructor to initialize dependencies
        public SupportTicketController(ICosmosDbService<SupportTicket> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }



        // POST: api/SupportTicket
        [HttpPost("CreateSupportTicket")]
        public async Task<IActionResult> CreateTicket([FromBody] SupportTicket SupportTicket)
        {
            if (SupportTicket == null)
            {
                return BadRequest("Ticket data cannot be null.");
            }

            // Assigning a new GUID for TicketId
            SupportTicket.SupportTicketId = Guid.NewGuid().ToString();
            SupportTicket.id = Guid.NewGuid().ToString();
            SupportTicket.status = "Open";
            SupportTicket.Date = DateTime.UtcNow;

            // Insert the Support ticket into Cosmos DB
            await _cosmosDbService.AddItemAsync(SupportTicket);
            return Ok(new { Message = "Support ticket created successfully", SupportTicketId = SupportTicket.SupportTicketId });
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuyProduct(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Successfully  deleted  SupportTickets  Item. ");
        }

        // GET: api/SupportTicket/{ticketId}
        [HttpGet("GetTicket/{ticketId}")]
        public async Task<IActionResult> GetTicket(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                return BadRequest("Ticket ID cannot be null or empty.");
            }

            var ticket = await _cosmosDbService.GetItemAsync(ticketId);
            if (ticket == null)
            {
                return NotFound($"Ticket with ID {ticketId} not found.");
            }

            return Ok(ticket);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<TrackTickets>> GetTicketByCustomerId(string customerId)
        {
            try
            {
                // Call the Cosmos DB service to get a ticket
                var ticket = await _cosmosDbService.GetRaiseTicketsAsync(customerId);

                // Check if a ticket was found
                if (ticket == null)
                {
                    return NotFound($"No tickets found for CustomerId: {customerId}.");
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                // Return internal server error if an exception occurs
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // POST: api/SupportTicket/EditTicket
        [HttpPost("EditTicket")]
        public async Task<IActionResult> EditTicket([FromBody] SupportTicket updatedTicket)
        {
            if (updatedTicket == null || string.IsNullOrEmpty(updatedTicket.SupportTicketId))
            {
                return BadRequest("Invalid ticket data.");
            }

            // Retrieve the existing ticket
            var existingTicket = await _cosmosDbService.GetItemAsync(updatedTicket.SupportTicketId);
            if (existingTicket == null)
            {
                return NotFound($"Ticket with ID {updatedTicket.SupportTicketId} not found.");
            }

            // Update the ticket fields
            existingTicket.Subject = updatedTicket.Subject ?? existingTicket.Subject;
            existingTicket.Category = updatedTicket.Category ?? existingTicket.Category;
            existingTicket.Address = updatedTicket.Address ?? existingTicket.Address;
            existingTicket.AssignedTo = updatedTicket.AssignedTo ?? existingTicket.AssignedTo;
            existingTicket.Attachments = updatedTicket.Attachments ?? existingTicket.Attachments; // Set attachments list

            // Save the updated ticket
            await _cosmosDbService.UpdateItemAsync(existingTicket);

            return Ok(new { Message = "Ticket updated successfully", Ticket = existingTicket });
        }


        [HttpGet("GetTicketsNotifications")]
        public async Task<IActionResult> GetTrackTicketDetails()
        {
            try
            {
                var SupportTicket = await _cosmosDbService.GetTrackTicketDetailsAsync();

                // Return 200 OK with tickets
                return Ok(SupportTicket);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error
                Console.WriteLine($"Error retrieving tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }
        //    [HttpGet("GetTotalCountOfSupportTicket")]
        //    public async Task<IActionResult> GetTotalCountOfSupportTicket()
        //    {
        //        try
        //        {
        //            // Call the service method to get ticket counts by status
        //            var ticketCounts = await _cosmosDbService.GetTotalCountsOfSupportTicket();

        //            if (ticketCounts == null)
        //            {
        //                return StatusCode(500, "Error fetching ticket counts.");
        //            }

        //            return Ok(ticketCounts); // Return the dictionary as a JSON response
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log the exception and return a generic error response
        //            Console.WriteLine($"Error: {ex.Message}");
        //            return StatusCode(500, "Unexpected error occurred.");
        //        }
        //    }

        //    [HttpGet("GetTotalCountOfSupportTicketBystateWise")]
        //    public async Task<ActionResult> GetTotalCountOfSupportTicketByStateWise(string state)
        //    {
        //        try
        //        {

        //            string normalisedstate = state.ToUpper();
        //            // Fetch the total count from the service
        //            var totalCount = await _cosmosDbService.GetTotalCountOfSupportTicketsByStateWise(normalisedstate);

        //            if (totalCount == null)
        //            {
        //                return StatusCode(500, "Error fetching total counts .");
        //            }
        //            return Ok(totalCount);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //            return StatusCode(500, "Unexpected  error occurred.");
        //        }
        //    }




        //    [HttpGet("GetTotalCountOfSupportTicketBystateWiseAndDistrictWise")]
        //    public async Task<IActionResult> GetTotalCountOfSupportTickeByStateWiseAndDistrictWise(string state, string district)
        //    {
        //        try
        //        {

        //            string normalisedstate = state.ToUpper();
        //            // Fetch the total count from the service
        //            var totalCount = await _cosmosDbService.GetTotalCountOfSupportTicketStateWiseAndDistrictWise(normalisedstate, district);

        //            // Return the total count in the desired format
        //            if (totalCount == null)
        //            {
        //                return StatusCode(500, "Error fetching  total counts.");
        //            }

        //            return Ok(totalCount);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //            return StatusCode(500, "Unexpected error occurred.");
        //        }
        //    }

        //    [HttpGet("GetTotalCountOfSupportTickeBystateWiseAndDistrictWiseAndZipCodeWise")]
        //    public async Task<IActionResult> GetTotalCountOfSupportTickeByStateWiseAndDistrictWiseAndZipcodeWise(string state, string district, string zipCode)
        //    {
        //        try
        //        {

        //            string normalisedstate = state.ToUpper();
        //            // Fetch the total count from the service
        //            var totalCount = await _cosmosDbService.GetTotalCountOfSupportTicketByStateWiseAndDistrictWiseAndZipcodeWise(normalisedstate, district, zipCode);

        //            if (totalCount == null)
        //            {
        //                return StatusCode(500, "Error fetching total counts");
        //            }
        //            return Ok(totalCount);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //            return StatusCode(500, "Unexpected  error occurred.");
        //        }


        //    }
        //}
    }
}