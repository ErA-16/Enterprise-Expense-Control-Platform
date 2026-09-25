using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Business_Operations_Expense_Control_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(DatabaseContext db, ILogger<NotificationController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDisplayDto>>> GetMyNotifications()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userIdClaim);

            _logger.LogInformation("User {Id} is fetching their notifications.", currentUserId);

            var notifications = await _db.Notifications
                .Where(n => n.ReceiverId == currentUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDisplayDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    RequestId = n.RequestId
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userIdClaim);

            var notification = await _db.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound("Notification not found");
            }

            if (notification.ReceiverId != currentUserId)
            {
                return Forbid();
            }

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Notification {Id} marked as read by user {UserId}.", id, currentUserId);

            return NoContent();
        }
    }
}
