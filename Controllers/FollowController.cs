using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using semproject.Services.Interfaces;

namespace semproject.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly IFollowService _followService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public FollowController(
            IFollowService followService,
            INotificationService notificationService,
            UserManager<IdentityUser> userManager)
        {
            _followService = followService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // Toggle follow/unfollow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string followingId)
        {
            var followerId = _userManager.GetUserId(User);

            try
            {
                var isNowFollowing = await _followService.ToggleFollowAsync(followerId, followingId);

                // Create notification when user follows someone
                if (isNowFollowing)
                {
                    await _notificationService.CreateFollowNotificationAsync(followerId, followingId);
                    TempData["SuccessMessage"] = "You are now following this user";
                }
                else
                {
                    TempData["SuccessMessage"] = "You have unfollowed this user";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            var referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index", "Home");
        }

        // Followers list
        public async Task<IActionResult> Followers(string userId, int page = 1)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                userId = currentUserId;

            var followers = await _followService.GetFollowersAsync(userId, page);
            var followerCount = await _followService.GetFollowerCountAsync(userId);

            ViewBag.UserId = userId;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.FollowerCount = followerCount;
            ViewBag.Page = page;

            return View(followers);
        }

        // Following list
        public async Task<IActionResult> Following(string userId, int page = 1)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                userId = currentUserId;

            var following = await _followService.GetFollowingAsync(userId, page);
            var followingCount = await _followService.GetFollowingCountAsync(userId);

            ViewBag.UserId = userId;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.FollowingCount = followingCount;
            ViewBag.Page = page;

            return View(following);
        }

        // Search users to follow
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new List<IdentityUser>());

            var currentUserId = _userManager.GetUserId(User);
            var users = await _followService.SearchUsersToFollowAsync(currentUserId, q);

            ViewBag.SearchQuery = q;
            return View(users);
        }

        // Suggested users to follow
        public async Task<IActionResult> Suggestions()
        {
            var currentUserId = _userManager.GetUserId(User);
            var suggestions = await _followService.GetSuggestedUsersAsync(currentUserId);

            return View(suggestions);
        }
    }
}