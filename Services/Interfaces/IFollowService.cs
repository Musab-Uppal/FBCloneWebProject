using Microsoft.AspNetCore.Identity;

namespace semproject.Services.Interfaces
{
    public interface IFollowService
    {
        Task<bool> ToggleFollowAsync(string followerId, string followingId);
        Task<bool> IsFollowingAsync(string followerId, string followingId);
        Task<int> GetFollowerCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
        Task<List<IdentityUser>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<IdentityUser>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<IdentityUser>> GetSuggestedUsersAsync(string userId, int limit = 10);
        Task<List<IdentityUser>> SearchUsersToFollowAsync(string currentUserId, string query);
    }
}