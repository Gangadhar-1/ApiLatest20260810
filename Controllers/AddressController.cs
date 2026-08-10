
using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Twilio.Rest.Api.V2010.Account;
using OtpAuthServices.Models;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ICosmosDbService<AddressModel> _cosmosDbService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(ICosmosDbService<AddressModel> cosmosDbService, ILogger<AddressController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("AddressUpload")]
        public async Task<IActionResult> UploadAddress([FromBody] AddressModel address)
        {
            if (address == null)
            {
                _logger.LogWarning("Address data is null.");
                return BadRequest("Address data cannot be null.");
            }

            try
            {
                address.AddressId = Guid.NewGuid().ToString();
                address.id = Guid.NewGuid().ToString();
                await _cosmosDbService.AddItemAsync(address);  // Add item to Cosmos DB

                _logger.LogInformation("Address uploaded successfully with ID {AddressId}", address.AddressId);

                return Ok(new
                {
                    Message = "Address data uploaded successfully",
                    AddressId = address.AddressId.ToString()  // Return as string in the response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading address.");
                return StatusCode(500, "An error occurred while uploading the address. Please try again.");
            }
        }


        [HttpGet("GetAddressById/{userId}")]

        public async Task<ActionResult<AddressModel>> GetAddressById(string userId)

        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required.");
            }




            try
            {
                // Fetch the address by UserId from Cosmos DB
                var address = await _cosmosDbService.GetAddress(userId);
                List<AddressModel> addresses = new List<AddressModel>();
                foreach (var addres in address)
                {
                    if (addres.IsPrimaryAddress == null)
                    {
                        addres.IsPrimaryAddress = true;

                    }
                    if (addres.AddressId == null)
                    {
                        addres.AddressId = addres.id;
                    }

                    addres.AddressId = addres.id;
                    addresses.Add(addres);
                }
                if (address == null)
                {
                    return NotFound($"Address with UserId {userId} not found.");
                }

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching address by UserId.");
                return StatusCode(500, "An error occurred while retrieving the address. Please try again.");
            }
        }



        [HttpGet("GetAddressById/{profileType}/{userId}")]
        public async Task<ActionResult<List<AddressModel>>> GetSecondaryAddressByProfileType(string profileType, string userId)
        {
            if (string.IsNullOrEmpty(profileType) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Both ProfileType and UserId are required.");
            }

            try
            {
                // Fetch the address by ProfileType and UserId from Cosmos DB
                var addresses = await _cosmosDbService.GetSecondaryAddress(profileType, userId);

                if (addresses == null || addresses.Count == 0)
                {
                    return NotFound($"No addresses found for ProfileType '{profileType}' and UserId '{userId}'.");
                }

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }





        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] AddressModel address)
        {
            if (address == null || address.id != id)
            {
                return BadRequest("Address information is incorrect. Please try again.");
            }

            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(address);
            return Ok("Address data Updated Successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return NoContent();
        }


        


    }
}
