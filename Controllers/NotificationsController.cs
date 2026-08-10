using Microsoft.AspNetCore.Mvc;

namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly FcmService _fcmService;

        public NotificationsController()
        {
            _fcmService = new FcmService();
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromQuery] string token)
        {
            await _fcmService.SendNotificationAsync(
                token,
                "New Handyman Request",
                "Someone needs help nearby!"
            );
            return Ok(new { success = true });
        }
    }

}
