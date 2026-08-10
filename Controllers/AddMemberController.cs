using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class AddMemberController : ControllerBase
    {
        private readonly ICosmosDbService<AddMember> _cosmosDbService;

        public AddMemberController(ICosmosDbService<AddMember> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        // Create a new product
        [HttpPost]
        [Route("AddMemberUpload")]
        public async Task<IActionResult> CreateAddMember([FromForm] AddMember addMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            addMember.id = Guid.NewGuid().ToString();
            addMember.AddMemberId = Guid.NewGuid().ToString();

            try
            {
                await _cosmosDbService.AddItemAsync(addMember);
                return Ok(new { Message = "AddMember data inserted successfully", MemberId = addMember.id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error saving data: {ex.Message}");
            }

        }

        [HttpGet("GetAddMember")]
        public async Task<IActionResult>  GetAddmember()
        {
            var addmember = await _cosmosDbService.GetAddMember();

           
            {
                
                return Ok(addmember);
            }
        }

        [HttpGet("GetAddMemberDetailsById")]
        public async  Task<IActionResult> GetAddMemberDetaisById(string id)
        {
            var addmember=await _cosmosDbService.GetAddMemberDetailsById(id);
            {
                return Ok(addmember);
            }
        }
     }
}

    