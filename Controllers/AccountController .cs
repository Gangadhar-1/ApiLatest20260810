using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static OtpAuthServices.Model.UploadFileResponse;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ICosmosDbService<MyAccounts> _cosmosDbService;

        public AccountController(ICosmosDbService<MyAccounts> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpPost]
        [Route("AccountGSTUpload")]
        public async Task<IActionResult> CreateAddAccountGST([FromForm] MyAccounts myAccounts)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var existAccountDetails = await _cosmosDbService.GetGSTAccountDetails(myAccounts.ProfileType, myAccounts.Category);

                if (existAccountDetails == null)
                {
                    myAccounts.id = Guid.NewGuid().ToString();
                    myAccounts.accountid = Guid.NewGuid().ToString();
                    await _cosmosDbService.AddItemAsync(myAccounts);
                    return Ok(new { Message = "Added GST Account Data  Successfully", MemberId = myAccounts.id });
                }
                else
                {
                    existAccountDetails.DeliveryCharge = myAccounts.DeliveryCharge;
                    existAccountDetails.ServiceCharge = myAccounts.ServiceCharge;
                    existAccountDetails.GstCharge = myAccounts.GstCharge;
                    existAccountDetails.OtherCharge = myAccounts.OtherCharge;

                    await _cosmosDbService.UpdateItemAsync(existAccountDetails);
                    return Ok("RaiseAQuote Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error saving data: {ex.Message}");
            }

        }

        [HttpGet("GetGSTAccountDetails/{profileType}/{category}")]
        public async Task<ActionResult<List<MyAccounts>>> GetGSTAccountDetails(string profileType, string category)
        {
            if (string.IsNullOrEmpty(profileType) || string.IsNullOrEmpty(category))
            {
                return BadRequest("Both ProfileType , category  are required.");
            }

            try
            {
                // Fetch the address by ProfileType and UserId from Cosmos DB
                var accountDetails = await _cosmosDbService.GetGSTAccountDetails(profileType, category);

                if (accountDetails == null)
                {
                    return NotFound($"No account Details found for ProfileType '{profileType}' and category '{category}'.");
                }

                return Ok(accountDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccountGST(string id, [FromBody] MyAccounts myAccounts)
        {
            if (myAccounts == null || myAccounts.id != id)
            {
                return BadRequest("GST Accounts information is incorrect. Please try again.");
            }

            var existingAccounts = await _cosmosDbService.GetItemAsync(id);
            if (existingAccounts == null)
            {
                return NotFound();
            }

            await _cosmosDbService.UpdateItemAsync(myAccounts);
            return Ok("GST Accounts data Updated Successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccountGST(string id)
        {
            var existingAccounts = await _cosmosDbService.GetItemAsync(id);
            if (existingAccounts == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return NoContent();
        }

    }
}

