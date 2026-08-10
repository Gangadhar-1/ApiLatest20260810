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
    public class DeliveryPartnerController : Controller
    {
        private readonly ICosmosDbService<DeliveryPartner> _cosmosDbService;
        private readonly ILogger<DeliveryPartnerController> _logger;

        public DeliveryPartnerController(ICosmosDbService<DeliveryPartner> cosmosDbService, ILogger<DeliveryPartnerController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadDeliveryPartnerDetails")]
        public async Task<IActionResult> UploadProductDetails([FromBody] DeliveryPartner deliveryPartner)
        {
            if (deliveryPartner == null)
            {
                return BadRequest("deliveryPartner cannot be null");
            }

            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "India Standard Time"
                    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                deliveryPartner.Date = indianTime.ToString("yyyy-MM-ddTHH:mm:ss");
                deliveryPartner.id = Guid.NewGuid().ToString();

                deliveryPartner.DeliveryPartnerId = Guid.NewGuid().ToString();


                await _cosmosDbService.AddItemAsync(deliveryPartner);

                return Ok(new
                {
                    message = "DeliveryPartner  Details uploaded successfully.",
                    id = deliveryPartner.id,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading DeliveryPartnerDetails.");
                return StatusCode(500, "An error occurred while uploading the DeliveryPartnerDetails. Please try again later.");
            }

        }




        [HttpPut("UpdateDeliveryPartnerDetails")]

        public async Task<IActionResult> UpdateDeliveryPartnerDetails(string id, [FromBody] DeliveryPartner deliveryPartner)
        {

            if (deliveryPartner == null)

            {
                return BadRequest("id can not be null");

            }

            var existingDeliveryPartner = await _cosmosDbService.GetItemAsync(id);

            if (existingDeliveryPartner == null)
            {
                return BadRequest("DeliveryPartner can not be null");

            }

            existingDeliveryPartner.id = deliveryPartner.id;
            existingDeliveryPartner.DeliveryPartnerId = deliveryPartner.DeliveryPartnerId;
            existingDeliveryPartner.DeliveryPartnerName = deliveryPartner.DeliveryPartnerName;
            existingDeliveryPartner.Photo = deliveryPartner.Photo;
            existingDeliveryPartner.Address = deliveryPartner.Address;
            existingDeliveryPartner.pancardNumber = deliveryPartner.pancardNumber;
            existingDeliveryPartner.AadharAttachment = deliveryPartner.AadharAttachment;
            existingDeliveryPartner.AadharCardNumber = deliveryPartner.AadharCardNumber;
            existingDeliveryPartner.DrivingLicense = deliveryPartner.DrivingLicense;
            existingDeliveryPartner.PhoneNumber = deliveryPartner.PhoneNumber;
            existingDeliveryPartner.Zipcode = deliveryPartner.Zipcode;
            existingDeliveryPartner.state = deliveryPartner.state;
            existingDeliveryPartner.Status = deliveryPartner.Status;
            existingDeliveryPartner.UserId = deliveryPartner.UserId;

            existingDeliveryPartner.district = deliveryPartner.district;



            await _cosmosDbService.UpdateItemAsync(existingDeliveryPartner);

            return Ok(new
            {

                message = "DeliveryPartner details Updated Successfully",

                Id = deliveryPartner.id,
            });

        }


        [HttpGet("GetDeliveryPartnerDetails/{id}")]
        public async Task<IActionResult> GetDeliveryPartnerDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("GetDeliveryPartnerDetails Id cannot be null or empty.");
            }

            var getDeliveryPartnerDetails = await _cosmosDbService.GetItemAsync(id);
            if (getDeliveryPartnerDetails == null)
            {
                return NotFound($"GetDeliveryPartnerDetails with ID {id} not found.");
            }

            return Ok(getDeliveryPartnerDetails);
        }



        [HttpGet("GetDeliveryPartnerDetailsByUserId")]


        public async Task<IActionResult> GetDeliveryPartnerDetailsByUserId(string userId)
        {

            if (string.IsNullOrEmpty(userId))

            {

                return BadRequest("UserId can not be null");
            }


            var existingdeliverpartner = await _cosmosDbService.GetDeliveryPartnerByUserId(userId);

            if (existingdeliverpartner == null)
            {

                return NotFound("existingdeliverpartner  can not be null");
            }

            return Ok(existingdeliverpartner);
        }



        [HttpGet("GetAllDeliveryPartners")]
        public async Task<IActionResult> GetAllDeliveryPartners()
        {


            var deliveryPartners = await _cosmosDbService.GetAllDeliveryPartners<DeliveryPartner>();
            if (deliveryPartners == null)
            {
                return NotFound($"deliveryPartners   not found.");
            }

            return Ok(deliveryPartners);
        }
    }

}