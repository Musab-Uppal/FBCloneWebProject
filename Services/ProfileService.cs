using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<UserProfile> GetUserProfileAsync(string userId, string currentUserId = null)
        {
            return await _profileRepository.GetUserProfileAsync(userId, currentUserId);
        }

        public async Task<UserStats> GetUserStatsAsync(string userId)
        {
            return await _profileRepository.GetUserStatsAsync(userId);
        }
    }
}