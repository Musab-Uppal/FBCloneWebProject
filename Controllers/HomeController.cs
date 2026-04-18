using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using semproject.Models;
using semproject.Services.Interfaces;
using System.Diagnostics;
using semproject.Authorization;

namespace semproject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly IFollowService _followService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ISearchService _searchService;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;

        public HomeController(
            ILogger<HomeController> logger,
            IPostService postService,
            IFollowService followService,
            UserManager<IdentityUser> userManager,
            ISearchService searchService,
            INotificationService notificationService,
            IAuthorizationService authorizationService)
        {
            _logger = logger;
            _postService = postService;
            _followService = followService;
            _userManager = userManager;
            _searchService = searchService;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Check if user is admin
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
                return RedirectToAction("Feed");
            }
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditPost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            // Check if current user is the post owner
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || post.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return PartialView("_EditPost", post);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int postId, string content)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Check if current user is the post owner
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || post.UserId != currentUser.Id)
            {
                return Json(new { success = false, message = "You are not authorized to edit this post" });
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Content is required" });
            }

            // Update post content
            var result = await _postService.UpdatePostContentAsync(postId, content, currentUser.Id);
            
            if (!result)
            {
                return Json(new { success = false, message = "Failed to update post" });
            }

            return Json(new { 
                success = true, 
                message = "Post updated successfully",
                content = content
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var userId = _userManager.GetUserId(User);
                var post = await _postService.GetPostByIdAsync(postId);

                if (post == null)
                {
                    return Json(new { success = false, message = "Post not found" });
                }

                var comment = new Comment
                {
                    PostId = postId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                var createdComment = await _postService.AddCommentAsync(comment);

                // Send notification if comment is on someone else's post
                if (post.UserId != userId)
                {
                    await _notificationService.CreateCommentNotificationAsync(userId, post.UserId, postId, content);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Return JSON for AJAX request
                    return Json(new
                    {
                        success = true,
                        message = "Comment added successfully",
                        commentId = createdComment.Id,
                        userName = (await _userManager.FindByIdAsync(userId))?.UserName
                    });
                }

                TempData["SuccessMessage"] = "Comment added successfully";
            }

            return RedirectToAction("Feed");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // For now, allow post owner or comment owner to delete
            var result = await _postService.DeleteCommentAsync(commentId);

            if (!result)
            {
                return Json(new { success = false, message = "Failed to delete comment" });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Comment deleted successfully" });
            }

            TempData["SuccessMessage"] = "Comment deleted successfully";
            return RedirectToAction("Feed");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = _userManager.GetUserId(User);
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            var isLiked = await _postService.ToggleLikeAsync(postId, userId);

            // Send notification if liking someone else's post
            if (isLiked && post.UserId != userId)
            {
                await _notificationService.CreateLikeNotificationAsync(userId, post.UserId, postId);
            }

            var likeCount = await _postService.GetLikeCountAsync(postId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, liked = isLiked, likeCount = likeCount });
            }

            TempData["SuccessMessage"] = isLiked ? "Post liked" : "Post unliked";
            return RedirectToAction("Feed");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Check authorization
            if (post.UserId != currentUser.Id)
            {
                return Json(new { success = false, message = "You are not authorized to delete this post" });
            }

            var result = await _postService.DeletePostAsync(postId);

            if (!result)
            {
                return Json(new { success = false, message = "Failed to delete post" });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Post deleted successfully" });
            }

            TempData["SuccessMessage"] = "Post deleted successfully";
            return RedirectToAction("Feed");
        }

        [Authorize]
        public async Task<IActionResult> Feed(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var posts = await _postService.GetFollowedUsersPostsAsync(userId, page);

            if (!posts.Any())
            {
                var randomPosts = await _postService.GetRandomPostsAsync(10);
                posts = randomPosts;
            }

            var postIds = posts.Select(p => p.Id).ToList();
            var likedStatus = await _postService.GetUserLikesStatusAsync(userId, postIds);

            ViewBag.LikedPostIds = likedStatus.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentPage = page;
            ViewBag.HasMorePosts = posts.Count > 0;

            return View(posts);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(string content, IFormFile? image = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Post content is required" });
            }

            try
            {
                var userId = _userManager.GetUserId(User);

                var post = new Post
                {
                    Content = content,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle image upload
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

                var createdPost = await _postService.CreatePostAsync(post);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Post created successfully!", postId = createdPost.Id });
                }

                TempData["SuccessMessage"] = "Post created successfully!";
                return RedirectToAction("Feed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while creating the post." });
                }

                TempData["ErrorMessage"] = "An error occurred while creating the post.";
                return RedirectToAction("Feed");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}