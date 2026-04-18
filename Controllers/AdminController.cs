using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using semproject.Models;
using semproject.Services.Interfaces;

namespace semproject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            IAdminService adminService,
            UserManager<IdentityUser> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            var users = _adminService.GetAllUsers();
            return View(users);
        }

        // POST: Admin/DeleteUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Prevent admin from deleting themselves
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Dashboard));
            }

            try
            {
                // Get username for success message
                var userName = await _adminService.GetUserNameByIdAsync(id);

                if (userName == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Dashboard));
                }

                await _adminService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = $"User '{userName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Database error: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // SIMPLER ALTERNATIVE: If you have ON DELETE CASCADE set up in SQL
    }

}