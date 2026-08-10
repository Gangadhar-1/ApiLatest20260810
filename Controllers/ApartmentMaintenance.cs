using Microsoft.AspNetCore.Mvc;
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
    public class ApartmentMaintenanceController : ControllerBase
    {
        private readonly ICosmosDbService<ApartmentMaintenance> _cosmosDbService;


        public ApartmentMaintenanceController(ICosmosDbService<ApartmentMaintenance> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }



        [HttpPost("CreateApartmentMaintence")]
        public async Task<IActionResult> CreateTicket([FromBody] ApartmentMaintenance apartmentMaintenance)
        {
            if (apartmentMaintenance == null)
            {
                return BadRequest("Ticket data cannot be null.");
            }

            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

            TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

            apartmentMaintenance.Date = indianTime.ToString("yyyy-MM-ddTHH:mm");


            apartmentMaintenance.id = Guid.NewGuid().ToString();
            apartmentMaintenance.UserId = Guid.NewGuid().ToString();





            await _cosmosDbService.AddItemAsync(apartmentMaintenance);
            return Ok(new { Message = "ApartmentMaintenance created successfully", ApartmentMaintenanceId = apartmentMaintenance.id });
        }


        [HttpGet]
        [Route("GetAddressMaintenanceDataByMobileNo")]
        public async Task<IActionResult> GetAddressMaintenanceDataByMobileNo(string mobileNo)
        {
            if (string.IsNullOrEmpty(mobileNo))
            {
                return BadRequest("mobile number  Not found.");
            }

            // Use the GetUserByEmailOrMobileAsync method to fetch the user
            var user = await _cosmosDbService.GetAddressMaintenanceDataByMobileNo(mobileNo);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("User not found.");
        }



        [HttpPost]
        [Route("ApartmentMaintenceEdit")]
        public async Task<IActionResult> ApartmentMaintenceEdit(ApartmentSubscription apartmentSubscription)
        {
            if (apartmentSubscription == null || string.IsNullOrEmpty(apartmentSubscription.id))
            {
                return BadRequest("ApartmentMaintenance information is incorrect or ID mismatch.");
            }

            ApartmentMaintenance ExistingapartmentSubscription = null;
            try
            {
                ExistingapartmentSubscription = await _cosmosDbService.GetItemAsync(apartmentSubscription.id);

                if (ExistingapartmentSubscription == null)
                {
                    return NotFound($"ApartmentMaintence with ID {apartmentSubscription.id} not found.");
                }
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
? "India Standard Time"
: "Asia/Kolkata";

            TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);
            try
            {
                if (ExistingapartmentSubscription.PaymentId != null)
                {
                    ExistingapartmentSubscription.PaymentId = apartmentSubscription.PaymentId;
                    //ExistingapartmentSubscription.MobileNumber = apartmentSubscription.MobileNumber;
                    //ExistingapartmentSubscription.ApartmentName = apartmentSubscription.ApartmentName;  
                    //ExistingapartmentSubscription.ApartmentAddress = apartmentSubscription.ApartmentAddress;    
                    //ExistingapartmentSubscription.PinCode = apartmentSubscription.PinCode;  
                    //ExistingapartmentSubscription.ConsentPersonName = apartmentSubscription.ConsentPersonName;  
                    //ExistingapartmentSubscription.NumberOfFlats = apartmentSubscription.NumberOfFlats;  
                    //ExistingapartmentSubscription.TotalAmount           = apartmentSubscription.TotalAmount;
                    ExistingapartmentSubscription.IsSubscription = apartmentSubscription.IsSubscription;

                    //ExistingapartmentSubscription.Status = "Open";

                    ExistingapartmentSubscription.SubscriptionDate = indianTime.ToString();
                    ExistingapartmentSubscription.PaidAmount = apartmentSubscription.PaidAmount;

                }
                else
                {
                    ExistingapartmentSubscription.Status = "Open";
                    ExistingapartmentSubscription.PaymentId = "";
                    ExistingapartmentSubscription.IsSubscription = "Subscription Failed!";
                }

                await _cosmosDbService.UpdateItemAsync(ExistingapartmentSubscription);
                return Ok(ExistingapartmentSubscription);
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"An error occurred while updating ApartmentMaintenance data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while updating ApartmentMaintenance data.");
            }
        }

        [HttpGet("GetApartmentMaintenance/{id}")]
        public async Task<IActionResult> GetApartmentMaintenance(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ApartmentMaintenance ID cannot be null or empty.");
            }

            var apartmentMaintenance = await _cosmosDbService.GetItemAsync(id);
            if (apartmentMaintenance == null)
            {
                return NotFound($"BookTechnician with ID {id} not found.");
            }

            return Ok(apartmentMaintenance);
        }


        [HttpGet("GetApartmentRegistrationsCount")]
        public async Task<IActionResult> GetApartmentRegistrationsCount()
        {
            try
            {
                var countsFromUser = await _cosmosDbService.GetApartmentRegistrationsCount();

                if (countsFromUser == null)
                {
                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
                }

               

                return Ok(new
                {
                    ApartmentRegistrationCount = countsFromUser.GetValueOrDefault("Open", 0),
                   

                 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApartmentMaintenance(string id, [FromBody] ApartmentMaintenance apartmentMaintenance)
        {
            if (apartmentMaintenance == null || apartmentMaintenance.id != id)
            {
                return BadRequest("ApartmentMaintenance information is incorrect.");
            }

            var existingapartmentMaintenance = await _cosmosDbService.GetItemAsync(id);
            if (existingapartmentMaintenance == null)
            {
                return NotFound("ApartmentMaintenance not found.");
            }

            // Allow TechnicianConfirmationCode to be set only once
            //if (string.IsNullOrEmpty(existingapartmentMaintenance.TechnicianConfirmationCode))
            //{
            //    existingbuyProduct.TechnicianConfirmationCode = GenerateRandomOtp(); // Set only if null/empty
            //}
            //else
            //{
            //    Console.WriteLine("TechnicianConfirmationCode update ignored. Using existing value.");
            //}

            // Other fields can still be updated
            existingapartmentMaintenance.ApartmentMaintenanceId = apartmentMaintenance.ApartmentMaintenanceId;
            existingapartmentMaintenance.ApartmentName = apartmentMaintenance.ApartmentName;

            existingapartmentMaintenance.ConsentPersonName = apartmentMaintenance.ConsentPersonName;

            existingapartmentMaintenance.ApartmentAddress = apartmentMaintenance.ApartmentAddress;
            existingapartmentMaintenance.SubscriptionDate = apartmentMaintenance.SubscriptionDate;

            existingapartmentMaintenance.NumberOfFlats = apartmentMaintenance.NumberOfFlats;

            existingapartmentMaintenance.TotalAmount = apartmentMaintenance.TotalAmount;

            existingapartmentMaintenance.IsSubscription = apartmentMaintenance.IsSubscription;

            existingapartmentMaintenance.PaymentId = apartmentMaintenance.PaymentId;
            existingapartmentMaintenance.PaidAmount = apartmentMaintenance.PaidAmount; 
            existingapartmentMaintenance.State = apartmentMaintenance.State;
            existingapartmentMaintenance.District = apartmentMaintenance.District;  


            await _cosmosDbService.UpdateItemAsync(existingapartmentMaintenance);

            return Ok(new
            {
                Message = "ApartmentMaintenance updated successfully",
                PaymentId = existingapartmentMaintenance.id,
            });
        }




    }
}
