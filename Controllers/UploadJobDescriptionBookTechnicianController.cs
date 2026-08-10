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
    public class UploadJobDescriptionBookTechnicianController : ControllerBase
    {
        private readonly ICosmosDbService<UploadJobDescriptionBookTechnician> _cosmosDbService;

        // Constructor to initialize dependencies
        public UploadJobDescriptionBookTechnicianController(ICosmosDbService<UploadJobDescriptionBookTechnician> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }




        [HttpPost("CreateUploadJobDescriptionBookTechnician")]
        public async Task<IActionResult> CreateTicket([FromBody] UploadJobDescriptionBookTechnician UploadJobDescriptionBookTechnician)
        {
            if (UploadJobDescriptionBookTechnician == null)
            {
                return BadRequest("Ticket data cannot be null.");
            }




            UploadJobDescriptionBookTechnician.id = Guid.NewGuid().ToString();


            // Insert the support ticket into Cosmos DB
            await _cosmosDbService.AddItemAsync(UploadJobDescriptionBookTechnician);
            return Ok(new { Message = "UploadJobDescriptionBookTechnician created successfully", UploadJobDescriptionBookTechnicianId = UploadJobDescriptionBookTechnician.id });
        }






        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadJobDescriptionBookTechnician(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Successfully  deleted  UploadJobDescriptionBookTechnician  Item. ");
        }


        [HttpGet("GetTicket/{id}")]
        public async Task<IActionResult> GetTicket(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Ticket ID cannot be null or empty.");
            }

            var ticket = await _cosmosDbService.GetItemAsync(id);
            if (ticket == null)
            {
                return NotFound($"Ticket with ID {id} not found.");
            }

            return Ok(ticket);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUploadJobDescriptionBookTechnician(string id, [FromBody] UploadJobDescriptionBookTechnician UploadJobDescriptionBookTechnician)
        {
            if (UploadJobDescriptionBookTechnician == null || UploadJobDescriptionBookTechnician.id != id)
            {
                return BadRequest("Product information is incorrect.");
            }

            var existingProduct = await _cosmosDbService.GetItemAsync(id);
            if (existingProduct == null)
            {
                existingProduct.id = UploadJobDescriptionBookTechnician.id;



            }
            await _cosmosDbService.UpdateItemAsync(UploadJobDescriptionBookTechnician);
            return Ok($"UploadJobDescriptionBookTechnician Data Updated Successfully. At with respectiveId {id}.");




        }


        [HttpGet("GetSelctedJobsByCategory")]
        public async Task<IActionResult> GetSelctedJobsByCategory(string Category)
        {
            if (string.IsNullOrEmpty(Category))
            {
                return BadRequest("Ticket ID cannot be null or empty.");
            }

            var SelctedJobsByCategory = await _cosmosDbService.GetSelctedJobsByCategory(Category);
            if (SelctedJobsByCategory == null)
            {
                return NotFound($"Required {Category} not found.");
            }

            return Ok(SelctedJobsByCategory);
        }




        [HttpGet("GetUploadJobDescriptionDetails")]
        public async Task<IActionResult> GetUploadJobDescriptionDetails()
        {
            try
            {
                var selectedJobs = await _cosmosDbService.GetUploadJobDescriptionDetails<UploadJobDescriptionBookTechnician>();

                if (selectedJobs == null || !selectedJobs.Any())
                {
                    return NotFound("No job descriptions found.");
                }

                return Ok(selectedJobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
    }