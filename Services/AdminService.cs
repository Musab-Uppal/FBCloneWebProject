using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public List<UserViewModel> GetAllUsers()
        {
            return _adminRepository.GetAllUsers();
        }

        public async Task<string> GetUserNameByIdAsync(string userId)
        {
            return await _adminRepository.GetUserNameByIdAsync(userId);
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _adminRepository.DeleteUserDataAsync(userId);
        }
    }
}
