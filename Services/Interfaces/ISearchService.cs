using Microsoft.AspNetCore.Identity;
using semproject.Models;

namespace semproject.Services.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResults> SearchAllAsync(string query, string currentUserId, int limit = 10);
        Task<List<Post>> GetRandomPostsAsync(int count = 10);
        Task<List<Group>> GetRandomGroupsAsync(int count = 6);
        Task<List<IdentityUser>> GetRandomUsersToFollowAsync(string currentUserId, int count = 8);
    }
}