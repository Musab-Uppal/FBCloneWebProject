using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using semproject.Models;
using semproject.Services.Interfaces;
using System.Security.Claims;

namespace semproject.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationController(
            INotificationService notificationService,
            UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Notification/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetNotificationsAsync(userId, page, pageSize);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            ViewBag.UnreadCount = unreadCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(notifications);
        }

        // GET: Notification/GetLatestNotifications
        public async Task<IActionResult> GetLatestNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetLatestNotificationsAsync(userId);

            return PartialView("_NotificationList", notifications);
        }

        // GET: Notification/GetUnreadCount
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Content(count.ToString());
        }

        // POST: Notification/MarkAsRead/{id}
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.MarkAsReadAsync(id, userId);

            return Json(new { success = result });
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _notificationService.MarkAllAsReadAsync(userId);

            return Json(new { success = true, count = count });
        }

        // GET: Notification/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _notificationService.GetNotificationAsync(id, userId);

            if (notification == null)
            {
                return NotFound();
            }

            // Mark as read when viewing details
            if (!notification.IsRead)
            {
                await _notificationService.MarkAsReadAsync(id, userId);
            }

            return View(notification);
        }

        // POST: Notification/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.DeleteNotificationAsync(id, userId);

            if (result)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }
    }
}