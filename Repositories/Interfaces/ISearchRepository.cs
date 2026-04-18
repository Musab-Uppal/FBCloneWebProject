using Microsoft.AspNetCore.Identity;
using semproject.Models;

namespace semproject.Repositories.Interfaces
{
    public interface ISearchRepository
    {
        Task<SearchResults> SearchAllAsync(string query, string currentUserId, int limit);
        Task<List<Post>> GetRandomPostsAsync(int count);
        Task<List<Group>> GetRandomGroupsAsync(int count);
        Task<List<IdentityUser>> GetRandomUsersToFollowAsync(string currentUserId, int count);
    }
}