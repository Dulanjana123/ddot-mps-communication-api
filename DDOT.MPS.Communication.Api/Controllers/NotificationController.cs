using DDOT.MPS.Communication.Api.Managers;
using DDOT.MPS.Communication.Model.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace DDOT.MPS.Communication.Api.Controllers
{
    [ApiController]
    public class NotificationController : CoreController
    {
        private readonly IRealTimeDataManager _realTimeDataManager;

        public NotificationController(IRealTimeDataManager realTimeDataManager)
        {
            _realTimeDataManager = realTimeDataManager;
        }

        [HttpPost("notify")]
        public async Task<IActionResult> Notify([FromBody] NotificationRequestDto request)
        {
            await _realTimeDataManager.Notify(request.Email, request.Type, request.Title, request.Body);
            return Ok(new { Message = "Notification sent successfully." });
        }

        [HttpPost("notify-global")]
        public async Task<IActionResult> NotifyGlobal([FromBody] GlobalNotificationRequestDto request)
        {
            // Send notification to all connected clients globally
            await _realTimeDataManager.SendGlobalNotification(request.Type, request.Title, request.Body);
            return Ok(new { Message = "Global notification sent successfully." });
        }
    }
}
