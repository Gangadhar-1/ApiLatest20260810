using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.Collections.Generic;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddTechnicianController : Controller
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<AddTechnician> _cosmosDbService;

        public AddTechnicianController(BlobService blobService, ICosmosDbService<AddTechnician> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }

        [HttpPost("AddTechnicianUpload")]
        public async Task<IActionResult> CreateAddTechnician([FromBody] AddTechnician addTechnician)
        {



            if (addTechnician == null)
            {

                return BadRequest("BuyProduct data cannot be null.");
            }

            try
            {
                addTechnician.AddTechnicianId = Guid.NewGuid().ToString();
                addTechnician.id = Guid.NewGuid().ToString();
                await _cosmosDbService.AddItemAsync(addTechnician);  // Add item to Cosmos DB



                return Ok(new
                {
                    Message = "AddTechnician data uploaded successfully",
                    AddTechnicianId = addTechnician.id.ToString()  // Return as string in the response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading address.");
                return StatusCode(500, "An error occurred while uploading the address. Please try again.");
            }
        }


        [HttpGet("GetAddTechnicianDetails")]

        public async Task<IActionResult> GetAddTechnicianDetails()
        {


            var addTechnician = await _cosmosDbService.GetAddTechnicians();
            {
                return Ok(addTechnician);
            }
        }


        [HttpGet("GetAddTechnicianDetailsById")]

        public async Task<IActionResult> GetAddTechnicianDetailsById(string id)
        {


            var addTechnician = await _cosmosDbService.GetAddTechnicianDetailsById(id);
            {
                return Ok(addTechnician);
            }
        }

        [HttpDelete("DeleteAddTechnicianDetails")]
        public async Task<IActionResult>  DeleteAddTechnicianDetails(string id)
        {
            var addtechnician = await _cosmosDbService.GetAddTechnicianDetailsById(id);
            if (addtechnician == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok ("Addtechnician  deleted successfully");
        }


    }
}

              