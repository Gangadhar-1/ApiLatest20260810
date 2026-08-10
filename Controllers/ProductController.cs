using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICosmosDbService<Product> _cosmosDbService;

        public ProductController(ICosmosDbService<Product> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        // Create a new product
        //[HttpPost]
        //[Route("ProductUpload")]
        //public async Task<IActionResult> CreateProduct([FromBody] Product product)
        //{
        //    if (product == null)
        //    {
        //        return BadRequest("Product cannot be null.");
        //    }

           
        //        product.id = Guid.NewGuid().ToString();
             
         

        //    if (string.IsNullOrEmpty(product.ProductStatus) && string.IsNullOrEmpty(product.ProductOwnedBy))
        //    {
        //        product.ProductStatus = "Approved";
        //        product.ProductOwnedBy = "Admin";
        //    }
        //    else if (string.IsNullOrEmpty(product.ProductStatus))
        //    {
        //        product.ProductStatus = "Draft";
        //    }



        //    await _cosmosDbService.AddItemAsync(product);
        //    return CreatedAtAction(nameof(GetProduct), new { id = product.id }, product);
        //}




        [HttpPost("ProductUpload")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {



            if (product == null)
            {

                return BadRequest("product data cannot be null.");
            }

            try
            {
                product.ProductId = Guid.NewGuid().ToString();
                product.id = Guid.NewGuid().ToString();
                product.ProductStatus  = "Pending Approval";
                product.ProductOwnedBy = "Admin";

                product.Date = DateTime.Now;
                await _cosmosDbService.AddItemAsync(product);  



                return Ok(new
                {
                    Message = "Product data uploaded successfully",
                    ProductId = product.id.ToString()  // Return as string in the response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading address.");
                return StatusCode(500, "An error occurred while uploading the address. Please try again.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _cosmosDbService.GetItemAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _cosmosDbService.GetItemsAsync();
            return Ok(products);
        }

        [HttpGet("GetProductList")]
        
        public async Task<ActionResult<List<AddressModel>>> GetProductList(string ProductOwnedBy)
        {
            if ((string.IsNullOrEmpty(ProductOwnedBy)))
            {
                return BadRequest("Both  Productstatus  and ProductOwnedBy are required.");
            }

            try
            {

                var Product = await _cosmosDbService.GetProductList(ProductOwnedBy);

                if (Product == null || Product.Count == 0)
                {
                    return NotFound($"No products   ProductStatus '{ProductOwnedBy}'.");
                }

                return Ok(Product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }



        [HttpGet("GetAdminProductList")]


        public async Task<ActionResult<List<AddressModel>>> GetAdminProductList(string ProductOwnedBy)
        {
            if ((string.IsNullOrEmpty(ProductOwnedBy)))
            {
                return BadRequest(" ProductOwnedBy is required.");
            }

            try
            {

                var Product = await _cosmosDbService.GetAdminProductList();

                if (Product == null || Product.Count == 0)
                {
                    return NotFound($"No products   ProductStatus '{ProductOwnedBy}'.");
                }

                return Ok(Product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }


        [HttpGet("GetAllProductList")]


        public async Task<ActionResult<List<AddressModel>>> GetAllProductList()
        {

            try
            {

                var Product = await _cosmosDbService.GetAllProductList();

                if (Product == null || Product.Count == 0)
                {
                    return NotFound($"No products   Found.");
                }

                return Ok(Product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }


        [HttpGet("GetProductsByCategory")]
        public async Task<ActionResult<List<AddressModel>>> GetProductsByCategory(string category)
        {
            if ((string.IsNullOrEmpty(category)))
            {
                return BadRequest(" category is required.");
            }

            try
            {

                var Product = await _cosmosDbService.GetProductNamesByCategory(category);

                if (Product == null || Product.Count == 0)
                {
                    return NotFound($"No products   found  with respective  '{category}'.");
                }

                return Ok(Product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses by ProfileType and UserId.");
                return StatusCode(500, "An error occurred while retrieving the addresses. Please try again.");
            }
        }





        // Update an existing product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
        {
            if (product == null || product.id != id)
            {
                return BadRequest("Product information is incorrect.");
            }

            var existingProduct = await _cosmosDbService.GetItemAsync(id);
            if (existingProduct == null)
            {
                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductPhotos = product.ProductPhotos;
                existingProduct.DeliveryInDays = product.DeliveryInDays;
                existingProduct.NumberOfStockAvailable = product.NumberOfStockAvailable;  
            }
            await _cosmosDbService.UpdateItemAsync(product); // Use the correct method signature
            return Ok($"Product Data Updated Successfully. At with respectiveId {id}.");
        }




        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
        //{
        //    // Validate incoming product object
        //    if (product == null)
        //    {
        //        return BadRequest("Product information is null.");
        //    }

        //    // Log the URL and body IDs for debugging
        //    Console.WriteLine($"URL ID: {id}");
        //    Console.WriteLine($"Body ID: {product.id}");

        //    // Validate that the ID in the URL matches the ID in the body
        //    if (product.id != id)
        //    {
        //        return BadRequest("Product ID in the URL does not match the body.");
        //    }

        //    try
        //    {
        //        // Fetch the existing product from Cosmos DB
        //        var existingProduct = await _cosmosDbService.GetItemAsync(id);
        //        if (existingProduct == null)
        //        {
        //            return NotFound($"Product with ID {id} not found.");
        //        }

        //        // Update fields in the existing product
        //        existingProduct.ProductName = product.ProductName;
        //        existingProduct.Category = product.Category;
        //        existingProduct.Catalogue = product.Catalogue;
        //        existingProduct.ProductSize = product.ProductSize;
        //        existingProduct.Color = product.Color;
        //        existingProduct.Units = product.Units;
        //        existingProduct.ProductPhotos = product.ProductPhotos;
        //        existingProduct.Rate = product.Rate;
        //        existingProduct.Discount = product.Discount;
        //        existingProduct.Specifications = product.Specifications;
        //        existingProduct.SpecificationDesc = product.SpecificationDesc;
        //        existingProduct.Warranty = product.Warranty;
        //        existingProduct.AdditionalInformation = product.AdditionalInformation;
        //        existingProduct.ProductStatus = product.ProductStatus;
        //        existingProduct.ProductOwnedBy = product.ProductOwnedBy;

        //        // Log the updated product for debugging
        //        Console.WriteLine($"Updated Product: {JsonConvert.SerializeObject(existingProduct)}");

        //        // Update the product in Cosmos DB
        //        await _cosmosDbService.UpdateItemAsync(product);

        //        // Return success status
        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception details
        //        Console.WriteLine($"Error in UpdateProduct: {ex.Message}");
        //        Console.WriteLine($"Stack Trace: {ex.StackTrace}");

        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the product.");
        //    }
        //}





        // Delete a product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var existingProduct = await _cosmosDbService.GetItemAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return NoContent();
        }
    }
}

// Upload product images
//[HttpPost("upload")]
//public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string productId)
//{
//    if (file == null || file.Length == 0)
//    {
//        return BadRequest("No file uploaded.");
//    }

//    // Specify your upload path (this should be a valid path in your environment)
//    var uploadPath = Path.Combine("YourUploadPath", file.FileName); // Change "YourUploadPath" to your actual path

//    using (var stream = new FileStream(uploadPath, FileMode.Create))
//    {
//        await file.CopyToAsync(stream);
//    }

//    // Add the uploaded image path to the product's Images list
//    var product = await _cosmosDbService.GetItemAsync(productId);
//    if (product == null)
//    {
//        return NotFound();
//    }

//    if (product.ProductPhoto == null)
//    {
//        product.ProductPhoto = new List<string>();
//    }

//    product.p.Add(uploadPath); // Add the image path to the product
//    await _cosmosDbService.UpdateItemAsync(product); // Use the correct method signature

//    return Ok(new { filePath = uploadPath });
//}
//    }
//}
