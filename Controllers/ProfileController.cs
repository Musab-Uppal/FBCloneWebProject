using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using semproject.Services.Interfaces;


namespace semproject.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IFollowService _followService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(
            IProfileService profileService,
            IFollowService followService,
            INotificationService notificationService,
            UserManager<IdentityUser> userManager)
        {
            _profileService = profileService;
            _followService = followService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // View user profile
        public async Task<IActionResult> View(string id)
        {
            if (string.IsNullOrEmpty(id))
                id = _userManager.GetUserId(User);

            var currentUserId = _userManager.GetUserId(User);
            var profile = await _profileService.GetUserProfileAsync(id, currentUserId);

            if (profile == null)
                return NotFound();

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.IsOwnProfile = (id == currentUserId);

            return View(profile);
        }

        // Get profile stats (for AJAX)
        [HttpGet]
        public async Task<IActionResult> GetStats(string userId)
        {
            var stats = await _profileService.GetUserStatsAsync(userId);
            return Json(stats);
        }

        // Toggle follow from profile
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string followingId)
        {
            var followerId = _userManager.GetUserId(User);

            try
            {
                var isNowFollowing = await _followService.ToggleFollowAsync(followerId, followingId);

                if (isNowFollowing)
                {
                    // Create notification
                    await _notificationService.CreateFollowNotificationAsync(followerId, followingId);
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("View", new { id = followingId });
        }
    }
}