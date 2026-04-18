using semproject.Models;

namespace semproject.Services.Interfaces
{
    public interface IAdminService
    {
        List<UserViewModel> GetAllUsers();
        Task<string> GetUserNameByIdAsync(string userId);
        Task DeleteUserAsync(string userId);
    }
}
