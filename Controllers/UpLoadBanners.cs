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
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Twilio.TwiML.Messaging;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpLoadBannners : Controller
    {
        private readonly ICosmosDbService<UploadBanners> _cosmosDbService;
        private readonly ILogger<UpLoadBannners> _logger;

        public UpLoadBannners(ICosmosDbService<UploadBanners> cosmosDbService, ILogger<UpLoadBannners> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadBanners")]
        public async Task<IActionResult> UploadBanners([FromBody] UploadBanners uploadBanners)
        {
            if (uploadBanners == null)
            {
                return BadRequest("uploadBanners cannot be null");
            }

            try
            {

                uploadBanners.id = Guid.NewGuid().ToString();



                await _cosmosDbService.AddItemAsync(uploadBanners);

                return Ok(new
                {
                    message = "uploadBanners uploaded successfully.",
                    id = uploadBanners.id,

                });

            }


            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading uploadBanners.");
                return StatusCode(500, "An error occurred while uploading the uploadBanners. Please try again later.");
            }

        }



        [HttpGet("GetBannerById/{id}")]
        public async Task<IActionResult> GetBannerById(string id)
        {

            //if (!string.IsNullOrEmpty(id))
            //{
            //    return BadRequest("Id can not be Null");
            //}

            var existingId = await _cosmosDbService.GetItemAsync(id);

            return Ok(existingId);

        }


        [HttpGet("GetBanners")]

        public async Task<IActionResult> GetBanners()
        {

            var banners = await _cosmosDbService.GetBanners();

            if (banners == null)
            {
                return NotFound("Get Banners can not be found");
            }

            return Ok(banners);
        }

        [HttpPut("UpdateBannerDetails")]

        public async Task<IActionResult> UpdateBannerDetails(string id, [FromBody] UploadBanners uploadBanners)
        {

            if (uploadBanners == null)

            {
                return BadRequest("id can not be null");

            }

            var existinguploadBanners = await _cosmosDbService.GetItemAsync(id);

            if (existinguploadBanners == null)
            {
                return BadRequest("uploadBanners can not be null");

            }

            existinguploadBanners.id = uploadBanners.id;
            existinguploadBanners.Title = uploadBanners.Title;
            existinguploadBanners.StartDate = uploadBanners.StartDate;
            existinguploadBanners.EndDate = uploadBanners.EndDate;
            existinguploadBanners.Description = uploadBanners.Description;
            existinguploadBanners.Image = uploadBanners.Image;
            existinguploadBanners.UpdatedDate = uploadBanners.UpdatedDate;

            


            await _cosmosDbService.UpdateItemAsync(existinguploadBanners);

            return Ok(new
            {

                message = "Banner details Updated Successfully",

                Id = existinguploadBanners.id,
            });

        }


        [HttpDelete("DeleteBanner/{id}")]

        public async Task<ActionResult> DeleteBanner(string id)

        {


            var existingbanner = await _cosmosDbService.GetItemsAsync(id);



            await _cosmosDbService.DeleteItemAsync(id);

            return Ok("Banner Deleted Successfullly");
        }



    }

}
