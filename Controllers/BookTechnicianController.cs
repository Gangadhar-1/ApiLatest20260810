using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using OtpAuthServices.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookTechnicianController : ControllerBase
    {
        private readonly ICosmosDbService<BookTechnician> _cosmosDbService;

        // Constructor to initialize dependencies
        public BookTechnicianController(ICosmosDbService<BookTechnician> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }




        [HttpPost("CreateBookTechnician")]
        public async Task<IActionResult> CreateTicket([FromBody] BookTechnician BookTechnician)
        {
            if (BookTechnician == null)
            {
                return BadRequest("Ticket data cannot be null.");
            }

            string ticketId = GenerateRaiseTicketId();


            BookTechnician.id = Guid.NewGuid().ToString();
            //BookTechnician.status = "Draft";
            BookTechnician.Date = DateTime.UtcNow;
            BookTechnician.BookTechnicianId = ticketId;

            // Insert the support ticket into Cosmos DB
            await _cosmosDbService.AddItemAsync(BookTechnician);
            return Ok(new { Message = "BookTechnician created successfully", RaiseTicketId = BookTechnician.id, BookTechnicianId = BookTechnician.BookTechnicianId });
        }


        private string GenerateRaiseTicketId()
        {
            Random random = new Random();
            string prefix = "BTKP"; // Fixed prefix
            string numbers = random.Next(1000, 9999).ToString(); // Random 4-digit number
            char letter = (char)random.Next('A', 'Z' + 1); // Random uppercase letter

            return $"{prefix}{numbers}{letter}";
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookTechnician(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok("Successfully  deleted BookTechnician   Item. ");
        }

        // GET: api/RaiseTicket/{ticketId}
        [HttpGet("GetBookTechnician/{id}")]
        public async Task<IActionResult> GetBookTechnician(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Ticket ID cannot be null or empty.");
            }

            var bookTechnician = await _cosmosDbService.GetItemAsync(id);
            if (bookTechnician == null)
            {
                return NotFound($"BookTechnician with ID {id} not found.");
            }

            return Ok(bookTechnician);
        }


        [HttpGet("GetBookTechnicianForAdminList")]
        public async Task<IActionResult> GetBookTechnicianListForAdmin()
        {
            var bookTechnicians = await _cosmosDbService.GetBookTechnicianListForAdmin<BookTechnician>();

            if (bookTechnicians == null || !bookTechnicians.Any())
            {
                return NotFound("No Book Technicians found.");
            }

            return Ok(bookTechnicians);
        }


        [HttpGet("GetBookTechnicianDetailsForUserList")]
        public async Task<IActionResult> GetBookTechnicianDetailsForUserList(string userId)
        {


            var buyProduct = await _cosmosDbService.GetBookTechnicianDetailsForUserList<BookTechnician>(userId);
            if (buyProduct == null)
            {
                return NotFound($"BuyProductId   not found.");
            }

            return Ok(buyProduct);
        }



        [HttpPost]
        [Route("bookTechnicianEdit")]
        public async Task<IActionResult> BookTechnicianEdit(PaymentRequest payment)
        {
            if (payment == null || string.IsNullOrEmpty(payment.id))
            {
                return BadRequest("BookTechnician information is incorrect or ID mismatch.");
            }

            BookTechnician existingBookTechnician = null;
            try
            {
                existingBookTechnician = await _cosmosDbService.GetItemAsync(payment.id);

                if (existingBookTechnician == null)
                {
                    return NotFound($"BookTechnician with ID {payment.id} not found.");
                }
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }

            try
            {
                if (payment.OrederId != null)
                {
                    existingBookTechnician.OrderId = payment.OrederId;
                    existingBookTechnician.OrderDate = payment.OrderDate;
                    existingBookTechnician.PaidAmount = payment.PaidAmount;
                    existingBookTechnician.TransactionStatus = payment.TransactionStatus;
                    existingBookTechnician.TransactionType = payment.TransactionType;
                    existingBookTechnician.InvoiceId = payment.InvoiceId;
                    existingBookTechnician.status = "Open";
                    existingBookTechnician.InvoiceURL = payment.InvoiceURL;
                    existingBookTechnician.UTRTransactionNumber = payment.UTRNumber;
                    //existingBookTechnician.NoOfQuantity = payment.NoOfQuantity;
                    //existingBookTechnician.TotalAmount = payment.TotalAmount;

                }
                else
                {
                    existingBookTechnician.status = "Open";
                    existingBookTechnician.UTRTransactionNumber = string.Empty;
                    existingBookTechnician.PaymentMode = "Payment Transaction Failed!";
                }

                await _cosmosDbService.UpdateItemAsync(existingBookTechnician);
                return Ok(existingBookTechnician);
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"An error occurred while updating BookTechnician data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while updating BookTechnician data.");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookTechnician(string id, [FromBody] BookTechnician BookTechnician)
        {
            if (BookTechnician == null || BookTechnician.id != id)
            {
                return BadRequest("BookTechnician information is incorrect.");
            }

            var existingBookTechnician = await _cosmosDbService.GetItemAsync(id);
            if (existingBookTechnician == null)
            {
                return NotFound("BookTechnician not found.");
            }

            // Allow TechnicianConfirmationCode to be set only once
            if (string.IsNullOrEmpty(existingBookTechnician.TechnicianConfirmationCode))
            {
                existingBookTechnician.TechnicianConfirmationCode = GenerateRandomOtp(); // Set only if null/empty
            }
            else
            {
                Console.WriteLine("TechnicianConfirmationCode update ignored. Using existing value.");
            }

            // Other fields can still be updated
            existingBookTechnician.BookTechnicianId = BookTechnician.BookTechnicianId;
            existingBookTechnician.PaymentMode = BookTechnician.PaymentMode;

            existingBookTechnician.UTRTransactionNumber = BookTechnician.UTRTransactionNumber;

            existingBookTechnician.TechnicianFullName = BookTechnician.TechnicianFullName;
            existingBookTechnician.AfterDiscount = BookTechnician.AfterDiscount;
            existingBookTechnician.JobDescription = BookTechnician.JobDescription;
            existingBookTechnician.TechnicianName = BookTechnician.TechnicianName;
            existingBookTechnician.TechnicianPincode = BookTechnician.TechnicianPincode;
            existingBookTechnician.NoOfQuantity = BookTechnician.NoOfQuantity;
            existingBookTechnician.TotalAmount = BookTechnician.TotalAmount;
            existingBookTechnician.status = BookTechnician.status;
            existingBookTechnician.AssignedTo = BookTechnician.AssignedTo;
            await _cosmosDbService.UpdateItemAsync(existingBookTechnician);

            return Ok(new
            {
                Message = "BookTechnician updated successfully",
                PaymentId = existingBookTechnician.id,
                TechnicianConfirmationCode = existingBookTechnician.TechnicianConfirmationCode
            });
        }




        private string GenerateRandomOtp()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                int randomNumber = BitConverter.ToInt32(bytes, 0);
                return (Math.Abs(randomNumber % 900000) + 100000).ToString("D6");
            }
        }

        [HttpGet("GetBookTechnicianNotifications")]
        public async Task<IActionResult> GetBookTechnicanNotifications(string category, string pincode, string technicianName)
        {
            var bookTechnicianNotification = await _cosmosDbService.GetBookTechnicianNotification<BookTechnician>(category, pincode, technicianName);
            if (bookTechnicianNotification == null || !bookTechnicianNotification.Any())
            {
                return NotFound("No BookTechnicanNotification Found");
            }
            return Ok(bookTechnicianNotification);
        }

    }

}



