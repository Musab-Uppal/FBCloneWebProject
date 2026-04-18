using Microsoft.AspNetCore.Identity;

namespace semproject.Repositories.Interfaces
{
    public interface IFollowRepository
    {
        Task<bool> ToggleFollowAsync(string followerId, string followingId);
        Task<bool> IsFollowingAsync(string followerId, string followingId);
        Task<int> GetFollowerCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
        Task<List<IdentityUser>> GetFollowersAsync(string userId, int page, int pageSize);
        Task<List<IdentityUser>> GetFollowingAsync(string userId, int page, int pageSize);
        Task<List<IdentityUser>> GetSuggestedUsersAsync(string userId, int limit);
        Task<List<IdentityUser>> SearchUsersToFollowAsync(string currentUserId, string query);
    }
}