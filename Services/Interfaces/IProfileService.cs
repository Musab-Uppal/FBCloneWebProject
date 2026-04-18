using Microsoft.AspNetCore.Identity;
using semproject.Models;

namespace semproject.Services.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfile> GetUserProfileAsync(string userId, string currentUserId = null);
        Task<UserStats> GetUserStatsAsync(string userId);
    }
}