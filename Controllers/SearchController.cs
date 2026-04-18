using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using semproject.Models;
using semproject.Services.Interfaces;

namespace semproject.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly IFollowService _followService;
        private readonly IGroupService _groupsService;
        private readonly UserManager<IdentityUser> _userManager;

        public SearchController(
            ISearchService searchService,
            IFollowService followService,
            IGroupService groupsService,
            UserManager<IdentityUser> userManager)
        {
            _searchService = searchService;
            _followService = followService;
            _groupsService = groupsService;
            _userManager = userManager;
        }

        // Unified search page
        public async Task<IActionResult> Index(string q, string type = "all")
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                // If no search query, show discover page
                return await Discover();
            }

            var currentUserId = _userManager.GetUserId(User);
            var results = await _searchService.SearchAllAsync(q, currentUserId);

            ViewBag.SearchQuery = q;
            ViewBag.SearchType = type;
            ViewBag.CurrentUserId = currentUserId;

            return View(results);
        }

        // Discover page with random content
        public async Task<IActionResult> Discover()
        {
            var currentUserId = _userManager.GetUserId(User);

            var viewModel = new DiscoverViewModel
            {
                RandomPosts = await _searchService.GetRandomPostsAsync(10),
                RandomGroups = await _searchService.GetRandomGroupsAsync(6),
                SuggestedUsers = await _searchService.GetRandomUsersToFollowAsync(currentUserId, 8)
            };

            ViewBag.CurrentUserId = currentUserId;
            return View("Discover", viewModel);
        }

        // AJAX search endpoint
        [HttpGet]
        public async Task<IActionResult> QuickSearch(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new { success = false });

            var currentUserId = _userManager.GetUserId(User);
            var results = await _searchService.SearchAllAsync(q, currentUserId, 5);

            return Json(new
            {
                success = true,
                users = results.Users.Select(u => new {
                    id = u.Id,
                    name = u.UserName,
                    email = u.Email
                }),
                posts = results.Posts.Select(p => new {
                    id = p.Id,
                    content = p.Content.Length > 50 ? p.Content.Substring(0, 50) + "..." : p.Content,
                    userName = p.User.UserName
                }),
                groups = results.Groups.Select(g => new {
                    id = g.Id,
                    name = g.Name,
                    memberCount = g.Members.Count
                })
            });
        }
    }
}