using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateDocumentController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ICosmosDbService<UpdateDocumentRequest> _cosmosDbService;

        public UpdateDocumentController(BlobService blobService, ICosmosDbService<UpdateDocumentRequest> cosmosDbService)
        {
            _blobService = blobService;
            _cosmosDbService = cosmosDbService;
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateDocument([FromBody] UpdateDocumentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Id))
            {
                return BadRequest("Invalid request. Id is required.");
            }

    
            string result = await _cosmosDbService.UpdateDocumentAsync(request);

   
            if (result == "Document not found!")
            {
                return NotFound(result);
            }

        
            return Ok(result);
        }

    }
}