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
    public class LakshmiMartController : Controller
    {
        private readonly ICosmosDbService<LakshmiMart> _cosmosDbService;


        public LakshmiMartController(ICosmosDbService<LakshmiMart> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }


        [HttpPost("UploadProductDetails")]

        public async Task<IActionResult> UploadProductDetails([FromBody] LakshmiMart lakshmiMart)
        {
            if (lakshmiMart == null)
            {
                return BadRequest("lakshmiMart can not be null");
            }

            try
            {
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                lakshmiMart.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");
                lakshmiMart.id = Guid.NewGuid().ToString();
                lakshmiMart.MartId = Guid.NewGuid().ToString();

                await _cosmosDbService.AddItemAsync(lakshmiMart);

                return Ok(new
                {
                    message = "ProductDetails uploaded id" + lakshmiMart.id + "successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ProductDetails.");
                return StatusCode(500, "An error occurred while uploading the ProductDetails. Please try again later.");
            }

        }


        //    [HttpGet("GetProductDetails")]

        //    public async Task<IActionResult> GetMartProductDetails (string id)
        //{
        //    {
        //        if (id == null) 
                
        //        {
        //            return BadRequest("Product Id can not be null");

        //    }

        //        var productdetails = await _cosmosDbService.GetItemAsync(id);

        //        if (productdetails == null)
        //        {

        //            return BadRequest("productdetails  can not be null");

        //        }

        //        return Ok(productdetails);  


        //    }



               }
    }

