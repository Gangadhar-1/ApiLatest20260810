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
    public class ReferralPointsController : Controller
    {
        private readonly ICosmosDbService<ReferralPoints> _cosmosDbService;
        private readonly ILogger<ReferralPointsController> _logger;

        public ReferralPointsController(ICosmosDbService<ReferralPoints> cosmosDbService, ILogger<ReferralPointsController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadReferralPoints")]
        public async Task<IActionResult> UploadReferralPoints([FromBody] ReferralPoints referralPoints)
        {
            if (referralPoints == null)
            {
                return BadRequest("referralPoints cannot be null");
            }

            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "India Standard Time"
                    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                referralPoints.Date = indianTime.ToString("yyyy-MM-ddTHH:mm:ss");
                referralPoints.id = Guid.NewGuid().ToString();


                await _cosmosDbService.AddItemAsync(referralPoints);

                return Ok(new
                {
                    message = "ReferralPoints uploaded successfully.",
                    id = referralPoints.id,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ReferralPoints.");
                return StatusCode(500, "An error occurred while uploading the ReferralPoints. Please try again later.");
            }
        }


        [HttpGet("GetReferralPointsByUserId")]
        public async Task<IActionResult> GetReferralPointsByUserId(string referreId)
        {
            if (string.IsNullOrEmpty(referreId))
            {
                return BadRequest("ReferreId cannot be null or empty.");
            }

            var referralPoints = await _cosmosDbService.GetReferralpointsByUserId(referreId);

            if (referralPoints == null || !referralPoints.Any())
            {
                return NotFound("No referral points found for this user.");
            }

            return Ok(referralPoints);
        }


        [HttpPut("UpdateReferralPoints")]

        public async Task<IActionResult> UpdateReferralPoints(string id, [FromBody] ReferralPoints referralPoints)

        {
            if (referralPoints == null)


            {
                return BadRequest("ReferralPoints can not be null");
            }
            var existingReferralPoints = await _cosmosDbService.GetItemAsync(id);

            if (existingReferralPoints == null)


            {
                return NotFound("ReferralPoints doesn't exist");
            }

            existingReferralPoints.referralNumbers = referralPoints.referralNumbers;
            existingReferralPoints.referralpoints = referralPoints.referralpoints;
            existingReferralPoints.referreId = referralPoints.referreId;
            existingReferralPoints.Date = referralPoints.Date;
            existingReferralPoints.IsReferralUsed = referralPoints.IsReferralUsed;  

            await _cosmosDbService.UpdateItemAsync(existingReferralPoints);


            return Ok(
                new
                {
                    Message = "Updated referral Points Successfully",

                    Id = id,
                }
                );


        }


    }

}


