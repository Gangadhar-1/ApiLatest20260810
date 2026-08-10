using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuyProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<BuyProduct> _cosmosDbService;

        public BuyProductController(BlobService blobService, ICosmosDbService<BuyProduct> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }

        [HttpPost("BuyProductUpload")]
        public async Task<IActionResult> UploadAddress([FromBody] BuyProduct buyProduct)
        {



            if (buyProduct == null)
            {

                return BadRequest("BuyProduct data cannot be null.");
            }

            try
            {


                var ticketId = GenerateRaiseTicketId();
                buyProduct.id = Guid.NewGuid().ToString();
                //buyProduct.status = "Open";

                buyProduct.BuyProductId = ticketId;
                await _cosmosDbService.AddItemAsync(buyProduct);  // Add item to Cosmos DB



                return Ok(new
                {
                    Message = "BuyProduct data uploaded successfully",
                    BuyProductId = buyProduct.id.ToString(),
                    BuyProductTicketId = buyProduct.BuyProductId // Return as string in the response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading address.");
                return StatusCode(500, "An error occurred while uploading the address. Please try again.");
            }
        }


        private string GenerateRaiseTicketId()
        {
            Random random = new Random();
            string prefix = "BPRF"; // Fixed prefix
            string numbers = random.Next(1000, 9999).ToString(); // Random 4-digit number
            char letter = (char)random.Next('A', 'Z' + 1); // Random uppercase letter

            return $"{prefix}{numbers}{letter}";
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuyProduct(string id, [FromBody] BuyProduct buyProduct)
        {

            if (buyProduct == null || buyProduct.id != id)
            {
                return BadRequest("BookTechnician information is incorrect.");
            }

            var existingbuyProduct = await _cosmosDbService.GetItemAsync(id);
            if (existingbuyProduct == null)
            {
                return NotFound("BookTechnician not found.");
            }

            // Allow TechnicianConfirmationCode to be set only once
            if (string.IsNullOrEmpty(existingbuyProduct.TechnicianConfirmationCode))
            {
                existingbuyProduct.TechnicianConfirmationCode = GenerateRandomOtp(); // Set only if null/empty
            }
            else
            {
                Console.WriteLine("TechnicianConfirmationCode update ignored. Using existing value.");
            }

            // Other fields can still be updated
            existingbuyProduct.BuyProductId = buyProduct.BuyProductId;
            existingbuyProduct.PaymentMode = buyProduct.PaymentMode;

            existingbuyProduct.UTRTransactionNumber = buyProduct.UTRTransactionNumber;

            existingbuyProduct.status = buyProduct.status;
            existingbuyProduct.AssignedTo = buyProduct.AssignedTo;

            existingbuyProduct.DeliveryCharges=buyProduct.DeliveryCharges;

            existingbuyProduct.ServiceCharges = buyProduct.ServiceCharges;      

            existingbuyProduct.TotalPaymentAmount = buyProduct.TotalPaymentAmount;  

            existingbuyProduct.DeliveryDate = buyProduct.DeliveryDate;      

            existingbuyProduct.TechnicianDetils=buyProduct.TechnicianDetils;

            existingbuyProduct.WarrentyPeriod = buyProduct.WarrentyPeriod;


            existingbuyProduct.UploadInvoice = buyProduct.UploadInvoice;

            existingbuyProduct.InvoiceDetails = buyProduct.InvoiceDetails;      
            await _cosmosDbService.UpdateItemAsync(existingbuyProduct);

            return Ok(new
            {
                Message = "BookTechnician updated successfully",
                PaymentId = existingbuyProduct.id,
                TechnicianConfirmationCode = existingbuyProduct.TechnicianConfirmationCode // Always return the correct value
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



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuyProduct(string id)
        {
            var existingaddress = await _cosmosDbService.GetItemAsync(id);
            if (existingaddress == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return   Ok("Successfully  deleted  BuyProduct  Item. ");
        }


        [HttpGet("GetBuyProductDetailsById/{id}")]
        public async Task<IActionResult> GetBuyProductDetailsById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("BuyProductId cannot be null or empty.");
            }

            var buyProduct = await _cosmosDbService.GetItemAsync(id);
            if (buyProduct == null)
            {
                return NotFound($"BuyProductId with ID {id} not found.");
            }

            return Ok(buyProduct);
        }


        [HttpGet("GetBuyProductDetailsForAdminList")]
        public async Task<IActionResult> GetBuyProductDetails()
        {
           

            var buyProduct = await _cosmosDbService.GetBuyProductDetailsForAdminList<BuyProduct>();
            if (buyProduct == null)
            {
                return NotFound($"BuyProductId   not found.");
            }

            return Ok(buyProduct);
        }



        [HttpGet("GetBuyProductDetailsForUserList")]
        public async Task<IActionResult> GetBuyProductDetailsForUserList(string userId)
        {


            var buyProduct = await _cosmosDbService.GetBuyProductDetailsForUserList<BuyProduct>(userId);
            if (buyProduct == null)
            {
                return NotFound($"BuyProductId   not found.");
            }

            return Ok(buyProduct);
        }






        [HttpGet("GetBuyProductDetailsByBuyProductId/{BuyProductId}")]
        public async Task<ActionResult<List<AddressModel>>> GetBuyProductDetails(string BuyProductId)
        {
            if (string.IsNullOrEmpty(BuyProductId))
            {
                return BadRequest("BuyProductId Is required.");
            }

            try
            {
                // Fetch the address by ProfileType and UserId from Cosmos DB
                var buyProduct = await _cosmosDbService.GetBuyProductdetails(BuyProductId);

                if (buyProduct == null || buyProduct.Count == 0)
                {
                    return NotFound($"No buyproducts   BuyProductId '{BuyProductId}'.");
                }

                return Ok(buyProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }


        
   


        [HttpGet("GetTotalCountOfBuyProducts")]
        public async Task<IActionResult> GetTotalCountOfBuyProducts()
        {
            try
            {
                // Call the service method to get ticket counts by status
                var ticketCounts = await _cosmosDbService.GetTotalCountOfBuyProducts();

                if (ticketCounts == null)
                {
                    return StatusCode(500, "Error fetching ticket counts.");
                }

                return Ok(ticketCounts); // Return the dictionary as a JSON response
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error response
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }


        [HttpGet("GetTotalCountOfBuyProductsBystateWise")]
        public async Task<IActionResult> GetTotalCountOfBuyProductsByStateWise(string state)
        {
            try
            {

                string normalisedstate = state.ToUpper();
                // Fetch the total count from the service
                var totalCounts = await _cosmosDbService.GetTotalCountOfBuyProductsByStateWise(normalisedstate);
                if(totalCounts == null)
                {
                    return StatusCode(500, "Error fetching total counts.");
                }

                return Ok(totalCounts);
                
            }
            catch  (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }



        [HttpGet("GetTotalCountOfBuyProductsBystateWiseAndDistrictWise")]
        public async Task<IActionResult> GetTotalCountOfBuyProductsByStateWiseAndDistrictWise(string state,string  district)
        {
            try
            {

                string normalisedstate = state.ToUpper();
                // Fetch the total count from the service
                var totalCount = await _cosmosDbService.GetTotalCountOfBuyProductsByStateWiseAndDistrictWise(normalisedstate,district);

                if (totalCount == null)
                {
                    return StatusCode(500, "Error fetching total counts.");
                }

                return Ok(totalCount);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }



        [HttpGet("GetTotalCountOfBuyProductsBystateWiseAndDistrictWiseAndZipCodeWise")]
        public async Task<ActionResult> GetTotalCountOfBuyProductsByStateWiseAndDistrictWiseAndZipcodeWise(string state, string district, string  zipCode)
        {
            try
            {

                string normalisedstate = state.ToUpper();
                // Fetch the total count from the service
                var totalCount = await _cosmosDbService.GetTotalCountOfBuyProductsByStateWiseAndDistrictWiseAndZipcodeWise(normalisedstate, district,zipCode);

                if (totalCount == null)
                {
                    return StatusCode(500, "Error fetching total counts.");
                }

                return Ok(totalCount);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }
        }



        [HttpPost]
        [Route("buyProductEdit")]
        public async Task<IActionResult> buyProductEdit(PaymentRequest payment)
        {

            if (payment == null)
            {
                return BadRequest($"BuyProduct information is incorrect or {payment.id} mismatch.");
            }

            BuyProduct existingBuyProduct = null;
            try
            {
                existingBuyProduct = await _cosmosDbService.GetItemAsync(payment.id);

                if (existingBuyProduct == null)
                {
                    return NotFound($"BuyProduct with ID {payment.id} not found.");
                }
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }
            if (payment.OrederId != null)
            {

                existingBuyProduct.OrderId = payment.OrederId;

                existingBuyProduct.OrderDate = payment.OrderDate;
                existingBuyProduct.PaidAmount = payment.PaidAmount;
                existingBuyProduct.TransactionStatus = payment.TransactionStatus;
                existingBuyProduct.TransactionType = payment.TransactionType;
                existingBuyProduct.InvoiceId = payment.InvoiceId;
                existingBuyProduct.InvoiceURL = payment.InvoiceURL;
                existingBuyProduct.status = "Open";
                existingBuyProduct.UTRTransactionNumber = payment.UTRNumber;

            }
            else
            {
                existingBuyProduct.status = "Open";
                existingBuyProduct.UTRTransactionNumber = "";
                existingBuyProduct.PaymentMode = "Payment Transaction Failed!";
            }

            try
            {
                await _cosmosDbService.UpdateItemAsync(existingBuyProduct);

                return Ok(existingBuyProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating BuyProduct data.");
            }
        }



    }
}