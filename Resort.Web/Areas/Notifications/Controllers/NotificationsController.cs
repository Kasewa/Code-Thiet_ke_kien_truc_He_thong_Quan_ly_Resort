using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Notifications.Controllers
{
    [Area("Notifications")]
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationOperations _notificationOps;

        public NotificationsController(INotificationOperations notificationOps)
        {
            _notificationOps = notificationOps;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationOps.GetNotificationsForUserAsync(CurrentUserId);
            return View(notifications);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string title, string content, string? recipientId)
        {
            var notification = new Notification
            {
                Title = title,
                Content = content,
                SenderId = CurrentUserId,
                RecipientId = recipientId ?? string.Empty,
                NotificationType = "General",
                CreatedBy = CurrentUserEmail,
                UpdatedBy = CurrentUserEmail
            };

            if (string.IsNullOrEmpty(recipientId))
                await _notificationOps.BroadcastNotificationAsync(notification);
            else
                await _notificationOps.CreateNotificationAsync(notification);

            TempData["Success"] = "Gửi thông báo thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(string id)
        {
            await _notificationOps.MarkAsReadAsync(id, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationOps.MarkAllAsReadAsync(CurrentUserId);
            TempData["Success"] = "Đã đánh dấu tất cả đã đọc!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _notificationOps.GetUnreadCountAsync(CurrentUserId);
            return Json(new { count });
        }
    }
}
