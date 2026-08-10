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
    public class LocationController : Controller
    {
        private readonly ICosmosDbService<Location> _cosmosDbService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ICosmosDbService<Location> cosmosDbService, ILogger<LocationController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadLocation")]
        public async Task<IActionResult> UploadProductDetails([FromBody] Location location)
        {
            if (location == null)
            {
                return BadRequest("location cannot be null");
            }

            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "India Standard Time"
                    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                location.Date = indianTime.ToString("yyyy-MM-ddTHH:mm:ss");
                location.id = Guid.NewGuid().ToString();


                await _cosmosDbService.AddItemAsync(location);

                return Ok(new
                {
                    message = "Location  details uploaded successfully.",
                    id = location.id,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading LocationDetails.");
                return StatusCode(500, "An error occurred while uploading the LocationDetails. Please try again later.");
            }
        }
    }
}
