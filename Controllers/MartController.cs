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
    public class MartController : Controller
    {
        private readonly ICosmosDbService<LakshmiMart> _cosmosDbService;
        private readonly ILogger<MartController> _logger;

        public MartController(ICosmosDbService<LakshmiMart> cosmosDbService, ILogger<MartController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [HttpPost("UploadProductDetails")]

        public async Task<IActionResult> UploadProductDetails([FromBody] LakshmiMart lakshmiMart)
        {
            if (lakshmiMart == null)
            {
                return BadRequest("lakshmiMart cannot be null");
            }

            try
            {
                var martId = GenerateGroceryBookingId();




                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "India Standard Time"
    : "Asia/Kolkata";

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);


                lakshmiMart.Date = indianTime.ToString("hh:mm tt dd-MM-yyyy");

                lakshmiMart.id = Guid.NewGuid().ToString();
                lakshmiMart.MartId = martId;

                await _cosmosDbService.AddItemAsync(lakshmiMart);

                return Ok(new
                {
                    message = "Product details uploaded successfully.",
                    id = lakshmiMart.id,
                    martId = lakshmiMart.MartId,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading ProductDetails.");
                return StatusCode(500, "An error occurred while uploading the ProductDetails. Please try again later.");
            }
        }

        private string GenerateGroceryBookingId()
        {
            Random random = new Random();
            string prefix = "LGM";
            string numbers = random.Next(100000, 999999).ToString();
            char letter = (char)random.Next('A', 'Z' + 1);

            return $"{prefix}{numbers}{letter}";
        }


        //[HttpPut("UpdateProductDetails/{id}")]
        //public async Task<IActionResult> UpdateProductDetails(string id, [FromBody] LakshmiMart lakshmiMart)
        //{
        //    if (lakshmiMart == null || lakshmiMart.id != id)
        //    {
        //        return BadRequest("LakshmiMart information is incorrect.");
        //    }

        //    try
        //    {
        //        var existinglaxmimart = await _cosmosDbService.GetItemAsync(id);

        //        if (existinglaxmimart == null)
        //        {
        //            return NotFound($"No LakshmiMart record found with id {id}.");
        //        }


        //        existinglaxmimart.customerId = lakshmiMart.customerId;

        //        existinglaxmimart.IsPickUp = lakshmiMart.IsPickUp;
        //        existinglaxmimart.IsDelivered = lakshmiMart.IsDelivered;
        //        existinglaxmimart.CustomerName = lakshmiMart.CustomerName;
        //        existinglaxmimart.CustomerPhoneNumber = lakshmiMart.CustomerPhoneNumber;
        //        existinglaxmimart.MartId = lakshmiMart.MartId;
        //        existinglaxmimart.Date = lakshmiMart.Date;
        //        existinglaxmimart.GrandTotal = lakshmiMart.GrandTotal;
        //        existinglaxmimart.TotalItemsSelected = lakshmiMart.TotalItemsSelected;
        //        existinglaxmimart.Address = lakshmiMart.Address;
        //        existinglaxmimart.State = lakshmiMart.State;
        //        existinglaxmimart.District = lakshmiMart.District;
        //        existinglaxmimart.ZipCode = lakshmiMart.ZipCode;
        //        existinglaxmimart.Status = lakshmiMart.Status;
        //        existinglaxmimart.PaymentMode = lakshmiMart.PaymentMode;
        //        existinglaxmimart.UTRTransactionNumber = lakshmiMart.UTRTransactionNumber;
        //        existinglaxmimart.TransactionNumber = lakshmiMart.TransactionNumber;
        //        existinglaxmimart.TransactionStatus = lakshmiMart.TransactionStatus;
        //        existinglaxmimart.PaidAmount = lakshmiMart.PaidAmount;
        //        existinglaxmimart.DeliveryPartnerUserId = lakshmiMart.DeliveryPartnerUserId;
        //        existinglaxmimart.AssignedTo = lakshmiMart.AssignedTo;
        //        existinglaxmimart.WalletAmount = lakshmiMart.WalletAmount;
        //        existinglaxmimart.latitude = lakshmiMart.latitude;
        //        existinglaxmimart.longitude = lakshmiMart.longitude;
        //        existinglaxmimart.SlotTime = lakshmiMart.SlotTime;
        //        //existinglaxmimart.Location = lakshmiMart.Location;
        //        existinglaxmimart.AvailedAmount = lakshmiMart.AvailedAmount;

        //      existinglaxmimart.TotalWalletAmount = lakshmiMart.TotalWalletAmount;

        //        existinglaxmimart.RemainingAmount   = lakshmiMart.  RemainingAmount;    

        //       existinglaxmimart.DeliveryAssignedTime=lakshmiMart.DeliveryAssignedTime;

        //      existinglaxmimart.DeliverySubmitTime = lakshmiMart.DeliverySubmitTime;  

        //        foreach (var updatedCategory in lakshmiMart.Categories)
        //        {
        //            var existingCategory = existinglaxmimart.Categories
        //                .FirstOrDefault(c => c.CategoryName == updatedCategory.CategoryName);

        //            if (existingCategory != null)
        //            {

        //                existingCategory.NumberOfItemsSelected = updatedCategory.NumberOfItemsSelected;
        //                existingCategory.TotalAmount = updatedCategory.TotalAmount;

        //                foreach (var updatedProduct in updatedCategory.Products)
        //                {
        //                    var existingProduct = existingCategory.Products
        //                        .FirstOrDefault(p => p.ProductName == updatedProduct.ProductName);

        //                    if (existingProduct != null)
        //                    {

        //                        existingProduct.NoOfQuantity = updatedProduct.NoOfQuantity;
        //                        existingProduct.ProductImage = updatedProduct.ProductImage;
        //                        existingProduct.MRP = updatedProduct.MRP;
        //                        existingProduct.Discount = updatedProduct.Discount;
        //                        existingProduct.StockLeft = updatedProduct.StockLeft;
        //                        existingProduct.AfterDiscountPrice = updatedProduct.AfterDiscountPrice;
        //                    }
        //                    else
        //                    {

        //                        existingCategory.Products.Add(updatedProduct);
        //                    }
        //                }
        //            }
        //            else
        //            {

        //                existinglaxmimart.Categories.Add(updatedCategory);
        //            }
        //        }


        //        await _cosmosDbService.UpdateItemAsync(lakshmiMart);

        //        return Ok(new
        //        {
        //            message = "Product details updated successfully.",
        //            id = existinglaxmimart.id,
        //            martId = existinglaxmimart.MartId
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error updating LakshmiMart record with id {id}");
        //        return StatusCode(500, "An error occurred while updating the product details. Please try again later.");
        //    }
        //}




        [HttpPut("UpdateProductDetails/{id}")]
        public async Task<IActionResult> UpdateProductDetails(string id, [FromBody] LakshmiMart lakshmiMart)
        {
            if (lakshmiMart == null || lakshmiMart.id != id)
            {
                return BadRequest("LakshmiMart information is incorrect.");
            }

            try
            {
                var existinglaxmimart = await _cosmosDbService.GetItemAsync(id);

                if (existinglaxmimart == null)
                {
                    return NotFound($"No LakshmiMart record found with id {id}.");
                }


                existinglaxmimart.customerId = lakshmiMart.customerId;

                existinglaxmimart.IsPickUp = lakshmiMart.IsPickUp;
                existinglaxmimart.IsDelivered = lakshmiMart.IsDelivered;
                existinglaxmimart.CustomerName = lakshmiMart.CustomerName;
                existinglaxmimart.CustomerPhoneNumber = lakshmiMart.CustomerPhoneNumber;
                existinglaxmimart.MartId = lakshmiMart.MartId;
                existinglaxmimart.Date = lakshmiMart.Date;
                existinglaxmimart.GrandTotal = lakshmiMart.GrandTotal;
                existinglaxmimart.TotalItemsSelected = lakshmiMart.TotalItemsSelected;
                existinglaxmimart.Address = lakshmiMart.Address;
                existinglaxmimart.State = lakshmiMart.State;
                existinglaxmimart.District = lakshmiMart.District;
                existinglaxmimart.ZipCode = lakshmiMart.ZipCode;
                existinglaxmimart.Status = lakshmiMart.Status;
                existinglaxmimart.PaymentMode = lakshmiMart.PaymentMode;
                existinglaxmimart.UTRTransactionNumber = lakshmiMart.UTRTransactionNumber;
                existinglaxmimart.TransactionNumber = lakshmiMart.TransactionNumber;
                existinglaxmimart.TransactionStatus = lakshmiMart.TransactionStatus;
                existinglaxmimart.PaidAmount = lakshmiMart.PaidAmount;
                existinglaxmimart.DeliveryPartnerUserId = lakshmiMart.DeliveryPartnerUserId;
                existinglaxmimart.AssignedTo = lakshmiMart.AssignedTo;
                existinglaxmimart.WalletAmount = lakshmiMart.WalletAmount;
                existinglaxmimart.latitude = lakshmiMart.latitude;
                existinglaxmimart.longitude = lakshmiMart.longitude;
                existinglaxmimart.SlotTime = lakshmiMart.SlotTime;
                //existinglaxmimart.Location = lakshmiMart.Location;
                existinglaxmimart.AvailedAmount = lakshmiMart.AvailedAmount;

                existinglaxmimart.TotalWalletAmount = lakshmiMart.TotalWalletAmount;

                existinglaxmimart.RemainingAmount = lakshmiMart.RemainingAmount;

                existinglaxmimart.DeliveryAssignedTime = lakshmiMart.DeliveryAssignedTime;

                existinglaxmimart.DeliverySubmitTime = lakshmiMart.DeliverySubmitTime;

                foreach (var updatedCategory in lakshmiMart.Categories)
                {
                    var existingCategory = existinglaxmimart.Categories
                        .FirstOrDefault(c => c.CategoryName == updatedCategory.CategoryName);

                    if (existingCategory != null)
                    {

                        existingCategory.NumberOfItemsSelected = updatedCategory.NumberOfItemsSelected;
                        existingCategory.TotalAmount = updatedCategory.TotalAmount;

                        foreach (var updatedProduct in updatedCategory.Products)
                        {
                            var existingProduct = existingCategory.Products
                                .FirstOrDefault(p => p.ProductName == updatedProduct.ProductName);

                            if (existingProduct != null)
                            {

                                existingProduct.NoOfQuantity = updatedProduct.NoOfQuantity;
                                existingProduct.ProductImage = updatedProduct.ProductImage;
                                existingProduct.MRP = updatedProduct.MRP;
                                existingProduct.Discount = updatedProduct.Discount;
                                existingProduct.StockLeft = updatedProduct.StockLeft;
                                existingProduct.AfterDiscountPrice = updatedProduct.AfterDiscountPrice;
                            }
                            else
                            {

                                existingCategory.Products.Add(updatedProduct);
                            }
                        }
                    }
                    else
                    {

                        existinglaxmimart.Categories.Add(updatedCategory);
                    }
                }


                //await _cosmosDbService.UpdateItemAsync(lakshmiMart);


                await _cosmosDbService.UpdateItemAsync(existinglaxmimart);
                return Ok(new
                {
                    message = "Product details updated successfully.",
                    id = existinglaxmimart.id,
                    martId = existinglaxmimart.MartId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating LakshmiMart record with id {id}");
                return StatusCode(500, "An error occurred while updating the product details. Please try again later.");
            }



        }




        [HttpGet("GetProductDetails")]

        public async Task<IActionResult> GetMartProductDetails(string id)
        {
            {
                if (id == null)

                {
                    return BadRequest("Product Id can not be null");

                }

                var productdetails = await _cosmosDbService.GetItemAsync(id);

                if (productdetails == null)
                {

                    return BadRequest("productdetails  can not be null");

                }

                return Ok(productdetails);


            }
        }

        [HttpDelete("Delete Products")]

        public async Task<IActionResult> Deleteproducts(string id)
        {

            if (!string.IsNullOrEmpty(id))
            {
                return BadRequest("Product id can not be null");
            }

            var existingproducts = await _cosmosDbService.GetItemAsync(id);

            if (existingproducts == null)
            {
                return BadRequest("Existingproduct can not be null");


            }

            await _cosmosDbService.DeleteItemAsync(id);

            return Ok("Successfully delete ProductItem");
        }


        [HttpPost("GroceryItemsEdit")]

        public async Task<IActionResult> GroceryItemsEdit(GroceryPaymentRequest groceryPaymentRequest)

        {

            if (groceryPaymentRequest == null)
            {
                return BadRequest($"GroceryPaymentRequest information is incorrect or {groceryPaymentRequest.id} mismatch.");
            }

            LakshmiMart existinglakshmiMart = null;

            try
            {
                existinglakshmiMart = await _cosmosDbService.GetItemAsync(groceryPaymentRequest.id);

                if (existinglakshmiMart == null)
                {
                    return NotFound($"LakshmiMart with ID {groceryPaymentRequest.id} not found.");
                }
            }
            catch (CosmosException ex)
            {
                return StatusCode(500, $"Error retrieving data from Cosmos DB: {ex.Message}");
            }


            if (groceryPaymentRequest.UTRTransactionNumber != null)
            {
                existinglakshmiMart.TransactionNumber = groceryPaymentRequest.TransactionNumber;

                existinglakshmiMart.UTRTransactionNumber = groceryPaymentRequest.UTRTransactionNumber;

                existinglakshmiMart.PaidAmount = groceryPaymentRequest.PaidAmount;
                existinglakshmiMart.TransactionStatus = groceryPaymentRequest.TransactionStatus;
                existinglakshmiMart.TransactionType = groceryPaymentRequest.TransactionType;

            }

            else
            {
                existinglakshmiMart.Status = "Open";
                existinglakshmiMart.UTRTransactionNumber = "";
                existinglakshmiMart.PaymentMode = "Payment Transaction Failed Please try again !";
            }


            try
            {
                await _cosmosDbService.UpdateItemAsync(existinglakshmiMart);

                return Ok(existinglakshmiMart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating BuyProduct data.");
            }
}

        [HttpGet("GetAllMartItems")]

        public async Task<IActionResult> GetAllMartItems()
        {

            var martitem = await _cosmosDbService.GetAllMartItems();

            if (martitem == null)
            {
                return NotFound("Mart items can not be found");
            }

            return Ok(martitem);
        }


        [HttpGet("GetMartTicketsByUserId")]
        public async Task<IActionResult> GetMartTicketsByUserId(string userId)
        {
            var martItems = await _cosmosDbService.GetMartTicketsByUserId(userId);

            if (martItems == null || !martItems.Any())
            {
                return NotFound("Mart items cannot be found");
            }

            return Ok(martItems);
        }



        [HttpGet("GetMartItemsByProductName")]
        public async Task<IActionResult> GetMartItemsByProductName(string productName)
        {
            var martItems = await _cosmosDbService.GetMartItemsByProductName(productName);

            if (martItems == null || !martItems.Any())
                return NotFound("Mart items cannot be found");

            return Ok(martItems);
        }


        [HttpGet("CheckFirstOrder")]

        public async Task<IActionResult> CheckFirstOrder(string CustomerPhoneNumber)
        {
            var firstorder = await _cosmosDbService.CheckFirstOrder(CustomerPhoneNumber);

            if (firstorder == null || !firstorder.Any())
            {
                return NoContent();
                
            }
            return Ok(firstorder);
        }
    }
}


    

