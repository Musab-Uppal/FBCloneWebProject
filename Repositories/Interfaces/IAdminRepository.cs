using semproject.Models;

namespace semproject.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        List<UserViewModel> GetAllUsers();
        Task<string> GetUserNameByIdAsync(string userId);
        Task DeleteUserDataAsync(string userId);
    }
}
