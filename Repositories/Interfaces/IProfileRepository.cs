using semproject.Models;

namespace semproject.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<UserProfile> GetUserProfileAsync(string userId, string currentUserId = null);
        Task<UserStats> GetUserStatsAsync(string userId);
    }
}