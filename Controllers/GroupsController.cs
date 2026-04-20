using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using semproject.Models;
using semproject.Services.Interfaces;

namespace semproject.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupsService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(
            IGroupService groupsService,
            UserManager<IdentityUser> userManager,
            IPostService postService,
            INotificationService notificationService,
            ILogger<GroupsController> logger)
        {
            _groupsService = groupsService;
            _userManager = userManager;
            _postService = postService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userGroups = await _groupsService.GetUserGroupsAsync(userId);
            var publicGroups = await _groupsService.GetPublicGroupsAsync();

            ViewBag.PublicGroups = publicGroups;
            ViewBag.CurrentUserId = userId;

            return View(userGroups);
        }

        public async Task<IActionResult> Browse(string search = "", string sort = "newest")
        {
            var userId = _userManager.GetUserId(User);
            List<Group> groups;

            if (!string.IsNullOrEmpty(search))
            {
                try
                {
                    groups = await _groupsService.SearchGroupsAsync(search);
                }
                catch
                {
                    var allGroups = await _groupsService.GetPublicGroupsAsync();
                    groups = allGroups.Where(g =>
                        g.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        g.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }
            else
            {
                groups = await _groupsService.GetPublicGroupsAsync();
            }

            // Apply sorting
            switch (sort)
            {
                case "popular":
                    groups = groups.OrderByDescending(g => g.MemberCount > 0 ? g.MemberCount : (g.Members?.Count ?? 0)).ToList();
                    break;
                case "name":
                    groups = groups.OrderBy(g => g.Name).ToList();
                    break;
                case "newest":
                default:
                    groups = groups.OrderByDescending(g => g.CreatedAt).ToList();
                    break;
            }

            // Get user's joined group IDs
            var userGroups = await _groupsService.GetUserGroupsAsync(userId);
            var joinedGroupIds = userGroups.Select(g => g.Id).ToList();

            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sort;
            ViewBag.JoinedGroupIds = joinedGroupIds;
            ViewBag.CurrentUserId = userId;

            return View(groups);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string description, bool isPrivate = false, IFormFile? coverImage = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Group name is required");
                return View();
            }

            var userId = _userManager.GetUserId(User);
            var group = await _groupsService.CreateGroupAsync(name, description, userId, isPrivate);

            // Handle cover image upload
            if (coverImage != null && coverImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine("wwwroot/uploads/groups", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }

                group.CoverImageUrl = $"/uploads/groups/{fileName}";
                await _groupsService.UpdateGroupAsync(group.Id, name, description, isPrivate, group.CoverImageUrl);
            }

            // Add creator as member
            await _groupsService.AddGroupMemberAsync(group.Id, userId);

            // Create notification for group creation
            await _notificationService.CreateGroupJoinNotificationAsync(userId, group.Id.ToString(), group.Name);

            TempData["SuccessMessage"] = "Group created successfully!";
            return RedirectToAction("Details", new { id = group.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var group = await _groupsService.GetGroupWithDetailsAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isMember = await _groupsService.IsUserMemberAsync(id, userId);
            var isAdmin = await _groupsService.IsUserAdminAsync(id, userId);

            // Get liked posts status
            var postIds = group.Posts.Select(gp => gp.Post?.Id ?? 0).Where(pid => pid > 0).ToList();
            var likedStatus = await _postService.GetUserLikesStatusAsync(userId, postIds);
            var likedPostIds = likedStatus.Where(kv => kv.Value).Select(kv => kv.Key).ToList();

            // Get like counts for each post
            var postLikes = new Dictionary<int, int>();
            foreach (var postId in postIds)
            {
                var likeCount = await _postService.GetLikeCountAsync(postId);
                postLikes[postId] = likeCount;
            }

            ViewBag.IsMember = isMember;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentUserId = userId;
            ViewBag.LikedPostIds = likedPostIds;
            ViewBag.PostLikes = postLikes;

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int groupId)
        {
            var userId = _userManager.GetUserId(User);

            try
            {
                await _groupsService.AddGroupMemberAsync(groupId, userId);

                // Create notification
                var group = await _groupsService.GetGroupByIdAsync(groupId);
                if (group != null)
                {
                    await _notificationService.CreateGroupJoinNotificationAsync(userId, groupId.ToString(), group.Name);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "You have joined the group!" });
                }

                TempData["SuccessMessage"] = "You have joined the group!";
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int groupId)
        {
            var userId = _userManager.GetUserId(User);

            try
            {
                await _groupsService.RemoveGroupMemberAsync(groupId, userId, userId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "You have left the group." });
                }

                TempData["SuccessMessage"] = "You have left the group.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SharePostToGroup(int groupId, int postId)
        {
            var userId = _userManager.GetUserId(User);

            try
            {
                var groupPost = await _groupsService.SharePostToGroupAsync(groupId, postId, userId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Post shared to group successfully!" });
                }

                TempData["SuccessMessage"] = "Post shared to group successfully!";
            }
            catch (UnauthorizedAccessException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while sharing the post." });
                }

                TempData["ErrorMessage"] = "An error occurred while sharing the post.";
            }

            return RedirectToAction("Details", new { id = groupId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateGroupPostView(int groupId)
        {
            var group = await _groupsService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isMember = await _groupsService.IsUserMemberAsync(groupId, userId);
            if (!isMember)
            {
                TempData["ErrorMessage"] = "You must be a member to post in this group.";
                return RedirectToAction("Details", new { id = groupId });
            }

            return View("CreateGroupPost", group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitGroupPostFromView(int groupId, string content, IFormFile? image = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Post content is required.";
                return RedirectToAction("CreateGroupPostView", new { groupId });
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var isMember = await _groupsService.IsUserMemberAsync(groupId, userId);

                if (!isMember)
                {
                    TempData["ErrorMessage"] = "You must be a member to post in this group.";
                    return RedirectToAction("Details", new { id = groupId });
                }

                var post = new Post
                {
                    Content = content,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine("wwwroot/uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    post.ImageUrl = $"/uploads/{fileName}";
                }

                await _postService.CreatePostAsync(post, isGroupPost: true, groupId: groupId);

                TempData["SuccessMessage"] = "Post created and shared to group successfully!";
                return RedirectToAction("Details", new { id = groupId });
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id = groupId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group post from full-page form for GroupId {GroupId}", groupId);
                TempData["ErrorMessage"] = "An error occurred while creating the post.";
                return RedirectToAction("CreateGroupPostView", new { groupId });
            }
        }
    }
}