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
    public class LakshmiCollectionController : Controller
    {
        private readonly ICosmosDbService<Collections> _cosmosDbService;
        private readonly ILogger<Collections> _logger;

        public LakshmiCollectionController(ICosmosDbService<Collections> cosmosDbService, ILogger<Collections> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }


        [HttpPost("UploadColectionsDetails")]

        public async Task<IActionResult> UploadColectionsDetails([FromBody] Collections collections)
        {
            if (collections == null)
            {
                return BadRequest("ColectionsDetails can not be null");
            }

            try
            {
                var collectionbookingId = GenerateLakshmiCollectionId();

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                collections.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");
                collections.id = Guid.NewGuid().ToString();
                collections.LakshmiCollectionId = collectionbookingId;

                await _cosmosDbService.AddItemAsync(collections);

                return Ok(new
                {
                    message = "ColectionsDetails uploaded successfully.",
                    Id = collections.id,
                    collectionbookingId = collections.LakshmiCollectionId,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ColectionsDetails.");
                return StatusCode(500, "An error occurred while uploading the ColectionsDetails. Please try again later.");
            }
        }



        private string GenerateLakshmiCollectionId()
        {
            Random random = new Random();
            string prefix = "LCB";
            string numbers = random.Next(1000, 9999).ToString();
            char letter = (char)random.Next('A', 'Z' + 1);

            return $"{prefix}{numbers}{letter}";
        }

        [HttpGet("GetLakshmiCollectionDetails/{id}")]
        public async Task<IActionResult> GetLakshmiCollectionDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("GetLakshmiCollectionDetails Id cannot be null or empty.");
            }

            var lakshmicollection = await _cosmosDbService.GetItemAsync(id);
            if (lakshmicollection == null)
            {
                return NotFound($"GetLakshmiCollectionDetails with ID {id} not found.");
            }

            return Ok(lakshmicollection);
        }


        [HttpPut("UpdateLakshmiCollectionDetails/{id}")]
        public async Task<IActionResult> UpdateLakshmiCollectionDetails(string id, [FromBody] Collections updatedCollection)
        {
            if (updatedCollection == null)
            {
                return BadRequest("LakshmiCollectionDetails cannot be null.");
            }

            try
            {
                var existingCollection = await _cosmosDbService.GetItemAsync(id);

                if (existingCollection == null)
                {
                    return NotFound($"LakshmiCollectionDetails not found for Id: {id}");
                }


                existingCollection.LakshmiCollectionId = updatedCollection.LakshmiCollectionId;
                existingCollection.GrandTotal = updatedCollection.GrandTotal;
                existingCollection.Status = updatedCollection.Status;
                existingCollection.Address = updatedCollection.Address;
                existingCollection.State = updatedCollection.State;

                existingCollection.Date = updatedCollection.Date;

                existingCollection.District = updatedCollection.District;
                existingCollection.ZipCode = updatedCollection.ZipCode;

                existingCollection.PaidAmount = updatedCollection.PaidAmount;

                existingCollection.PaymentMode = updatedCollection.PaymentMode;

                existingCollection.UTRTransactionNumber = updatedCollection.UTRTransactionNumber;
                existingCollection.TransactionNumber = updatedCollection.TransactionNumber;

                existingCollection.TransactionType = updatedCollection.TransactionType;

                foreach (var updatedCategory in updatedCollection.categoriess)
                {
                    var existingCategory = existingCollection.categoriess
                        .FirstOrDefault(c => c.CategoryName == updatedCategory.CategoryName);

                    if (existingCategory != null)
                    {

                      

                                existingCategory.CategoryName = updatedCategory.CategoryName;
                        existingCategory.ProductName = updatedCategory.ProductImage;
                                existingCategory.NoOfQuantity = updatedCategory.NoOfQuantity;
                                existingCategory.Discount = updatedCategory.Discount;
                                existingCategory.StockLeft = updatedCategory.StockLeft;
                                existingCategory.AfterDiscountPrice = updatedCategory.AfterDiscountPrice;
                        existingCategory.size = updatedCategory.size;
                        existingCategory.code = updatedCategory.code;
                        existingCategory.colour = updatedCategory.colour;
                        existingCategory.ProductImage = updatedCategory.ProductImage;
                            }
                            else
                            {

                                existingCollection.categoriess.Add(updatedCategory);
                            }
                        }
                    
                


                await _cosmosDbService.UpdateItemAsync(updatedCollection);

                return Ok(new
                {
                    message = "Product details updated successfully.",
                    id = updatedCollection.id,
                    martId = updatedCollection.LakshmiCollectionId,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating LakshmiMart record with id {id}");
                return StatusCode(500, "An error occurred while updating the product details. Please try again later.");
            }
        }

        [HttpGet("GetAllLakshmiCollectionsOpen")]

        public async Task<IActionResult> GetAllLakshmiCollectionsOpen()
        {
            var lcollections = await _cosmosDbService.GetAllLakshmiCollectionsByopoen();

            if(lcollections == null)
            {
                return BadRequest("LakshmiCollections not found");
            }

            return Ok(lcollections);    

        }

        [HttpGet("GetLakshmicollectionsById")]

        public async Task<IActionResult> GetLakshmicollectionsById(string id)
        {

            if(string.IsNullOrEmpty(id))
            {
                return BadRequest("Id can not be empty");
            }

            var getcollections =await _cosmosDbService.GetItemAsync(id);

            return Ok(getcollections);
        }
        }
        }





