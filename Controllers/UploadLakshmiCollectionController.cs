using Azure.Storage.Blobs.Models;
using Google.Apis.Auth.OAuth2;
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
    public class UploadLakshmiCollectionController : Controller
    {
        private readonly ICosmosDbService<Lakshmincollection> _cosmosDbService;

        public UploadLakshmiCollectionController(ICosmosDbService<Lakshmincollection> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("UploadLakshmiCollection")]

        public async Task<IActionResult> UploadGrocery([FromBody] Lakshmincollection lakshmincollection)
        {
            if (lakshmincollection == null)
            {
                return BadRequest("UploadLakshmiCollection can not be null");
            }

            try
            {
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                lakshmincollection.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");
                lakshmincollection.id = Guid.NewGuid().ToString();
                lakshmincollection.LakshmicollectionId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(lakshmincollection);

                return Ok(new
                {
                    message = "LakshmiCollection uploaded successfully.",
                    Id = lakshmincollection.id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading LakshmiCollection.");
                return StatusCode(500, "An error occurred while uploading the LakshmiCollection. Please try again later.");
            }
        }


        [HttpGet("GetLakshmiCollections")]

        public async Task<IActionResult> GetLakshmiCollections(string id)
        {
            if (id == null)
            {
                return BadRequest("Id can not be null");
            }

            var lakshmicollection = await _cosmosDbService.GetItemAsync(id);

            if (lakshmicollection == null)
            {
                return NotFound("Lakshmicollection data not found");
            }

            return Ok(lakshmicollection);

        }

        [HttpGet("GetAllLakshmiCollectionsByCategory")]
        public async Task<IActionResult> GetAllLakshmiCollectionsByCategory(string category)
        {
            if (category == null)

            {
                return BadRequest("Category can not be null");
            }

            var collections = await _cosmosDbService.GetLakshmiCollectionByCategory(category);

            if (collections == null)
            {
                return NotFound("Lakshmi Collections not found");
            }

            return Ok(collections);
        }


        [HttpPut("UpdateLakshmiCollection")]

        public async Task<IActionResult> UpdateLakshmiCollection(string id, [FromBody] Lakshmincollection lakshmincollection)
        {

            if (lakshmincollection == null)
            {
                return BadRequest("Lakshmincollection can not be null");
            }

            var existinglakshmicollection = await _cosmosDbService.GetItemAsync(id);


            if (existinglakshmicollection == null)

            {
                return NotFound("Lakshmicollection not found");
            }



            existinglakshmicollection.id = lakshmincollection.id;
            existinglakshmicollection.Date = lakshmincollection.Date;
            existinglakshmicollection.ProductName = lakshmincollection.ProductName;

            existinglakshmicollection.Category = lakshmincollection.Category;
            existinglakshmicollection.Catalogue = lakshmincollection.Catalogue;
            existinglakshmicollection.size = lakshmincollection.size;
            existinglakshmicollection.colour = lakshmincollection.colour;
            existinglakshmicollection.Rate = lakshmincollection.Rate;

            existinglakshmicollection.Discount = lakshmincollection.Discount;

            existinglakshmicollection.AfterDiscount = lakshmincollection.AfterDiscount;

            existinglakshmicollection.Optional = lakshmincollection.Optional;
            existinglakshmicollection.MoreInfo = lakshmincollection.MoreInfo;

            existinglakshmicollection.DeliveryInDays = lakshmincollection.DeliveryInDays;

            existinglakshmicollection.StockLeft = lakshmincollection.StockLeft;

            await _cosmosDbService.UpdateItemAsync(lakshmincollection);


            return Ok(new
            {

                message = "Lakshmincollection Updated Successfully",
                Id = lakshmincollection.id
            });

        }

        [HttpGet("GetAllLakshmiCollections")]

        public async Task<IActionResult> GetAllLakshmiCollection()
        {

            var collections = await _cosmosDbService.GetAllLakshmiCollections();

            if (collections == null)
            {
                return NotFound("No data found");
            }

            return Ok(collections);

        }


        [HttpGet("GetLakshmiCollectionsItemByProductName")]

        public async Task<IActionResult> GetLakshmiCollectionsItemByProductName(string productName)
        {
            var collection = await _cosmosDbService.GetcollectionItemsByproductName(productName);

            if (collection == null)
            {
                return NotFound("No data found");
            }
            return Ok(collection);

        }
    }
}




    
