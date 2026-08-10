using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using OtpAuthServices.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Twilio.TwiML.Messaging;


namespace OtpAuthServices.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UploadGroceryController : Controller
    {
        private readonly ICosmosDbService<UploadGrocery> _cosmosDbService;

        public UploadGroceryController(ICosmosDbService<UploadGrocery> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("UploadGrocery")]

        public async Task<IActionResult> UploadGrocery([FromBody] UploadGrocery uploadGrocery)
        {
            if (uploadGrocery == null)
            {
                return BadRequest("UploadGrocery can not be null");
            }

            try
            {
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                uploadGrocery.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");
                uploadGrocery.id = Guid.NewGuid().ToString();
                uploadGrocery.GroceryItemId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(uploadGrocery);

                return Ok(new
                {
                    message = "GroceryItem uploaded id" + uploadGrocery.id + "successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading GroceryItem.");
                return StatusCode(500, "An error occurred while uploading the GroceryItem. Please try again later.");
            }

        }

        

        [HttpGet("GetGroceryItems/{id}")]
        public async Task<IActionResult> GetGroceryItems(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("GetGroceryItems Id cannot be null or empty.");
            }

            var getGroceryItems = await _cosmosDbService.GetItemAsync(id);
            if (getGroceryItems == null)
            {
                return NotFound($"GetGroceryItems with ID {id} not found.");
            }

            return Ok(getGroceryItems);
        }


        [HttpGet("GetGroceryItemsBycategory")]

        public async Task<IActionResult> GetGroceryItemsBycategory(string Category)
        {
            if (string.IsNullOrEmpty(Category))
            {
                return BadRequest("Category Can not be null");
            }

            var getGroceryItemsBycategory = await _cosmosDbService.GetGroceryItemsByCategory(Category);

            if (getGroceryItemsBycategory == null)
            {
                return NotFound("GroceryItemsBycategory not found");
            }
            return Ok(getGroceryItemsBycategory);
        }

        [HttpGet("GetAllGroceryItems")]

        public async Task<IActionResult> GetAllGroceryItems()
        {
            var getallItems=await _cosmosDbService.GetAllGroceryItems();

            if(getallItems == null)
            {
                return NotFound("GroceryItems not found");
            }

            return Ok(getallItems);
        }




        [HttpGet("GetAllGroceryItemsForAdmin")]

        public async Task<IActionResult> GetAllGroceryItemsForAdmin()
        {
            var getallItems = await _cosmosDbService.GetAllGroceryItemsForAdmin();

            if (getallItems == null)
            {
                return NotFound("GroceryItems not found");
            }

            return Ok(getallItems);
        }

        [HttpGet("GetGroceryItemsByProductName")]

        public async Task<IActionResult> GetGroceryItemsByProductName(string productName)
        {

            productName = productName?.Trim();  
            var getGroceryItems = await _cosmosDbService.GetGroceryItemsByproductName(productName);

            if (getGroceryItems == null)
            {
                return NotFound("GroceryItems  not found");
            }
            return Ok(getGroceryItems);
        }



        [HttpPut("UpdateGroceryItems")]

        public async Task<IActionResult> UpdateGroceryItems(string id, [FromBody] UploadGrocery uploadGrocery)
        {
            if (uploadGrocery == null)

            {
                return BadRequest("id can not be null");

            }

            var existingGroceryItem = await _cosmosDbService.GetItemAsync(id);

            if (existingGroceryItem == null)
            {
                return BadRequest("GroceryItem can not be null");

            }

            existingGroceryItem.id = uploadGrocery.id;
            existingGroceryItem.Category = uploadGrocery.Category;
            existingGroceryItem.Name = uploadGrocery.Name;
            existingGroceryItem.MRP = uploadGrocery.MRP;
            existingGroceryItem.Discount = uploadGrocery.Discount;
            existingGroceryItem.AfterDiscount = uploadGrocery.AfterDiscount;
            existingGroceryItem.Date = uploadGrocery.Date;
            existingGroceryItem.Images = uploadGrocery.Images;
            existingGroceryItem.StockLeft = uploadGrocery.StockLeft;
            existingGroceryItem.DeliveryIn = uploadGrocery.DeliveryIn;
            existingGroceryItem.Status = uploadGrocery.Status;
            existingGroceryItem.Code=uploadGrocery.Code;
            existingGroceryItem.Units= uploadGrocery.Units; 
            existingGroceryItem.RequestedBy = uploadGrocery.RequestedBy;

            existingGroceryItem.Limit = uploadGrocery.Limit;

            //existingGroceryItem.selectedZipcodes = uploadGrocery.selectedZipcodes;

            await _cosmosDbService.UpdateItemAsync(existingGroceryItem);

            return Ok(new
            {

                message = "GroceryItem Updated" + uploadGrocery.id + "Successfully",
            });

        }



        [HttpDelete]
         public async Task<IActionResult> DeleteGroceryItem(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("id can not be null");
            }
             var existingGroceryItem = await _cosmosDbService.GetItemsAsync(id);

            if (existingGroceryItem == null)
            {
                return NotFound("existingGroceryItem not found");
            }
                await _cosmosDbService.DeleteItemAsync(id);

                return Ok("Deleted GroceryItem Successfully");
           }
    }
}

