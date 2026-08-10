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


namespace OtpAuthServices.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LmartLogsController : Controller
    {

        private readonly ICosmosDbService<LmartLogs> _cosmosDbService;

        private readonly ILogger<LmartLogsController> _logger;
        public LmartLogsController(ICosmosDbService<LmartLogs> cosmosDbService, ILogger<LmartLogsController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadlogsDetails")]
        public async Task<IActionResult> UploadlogsDetails([FromBody] LmartLogs lmartLogs)
        {
            if (lmartLogs == null)
            {
                return BadRequest("lmartLogs cannot be null");
            }

            try
            {


                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);


                lmartLogs.Date = indianTime.ToString("hh:mm tt dd-MM-yyyy");

                lmartLogs.id = Guid.NewGuid().ToString();


                await _cosmosDbService.AddItemAsync(lmartLogs);

                return Ok(new
                {
                    message = "Logs details uploaded successfully.",
                    id = lmartLogs.id,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading LogsDetails.");
                return StatusCode(500, "An error occurred while uploading the LogsDetails. Please try again later.");
            }
        }
    }
}
