using Microsoft.AspNetCore.Identity;
using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class SearchService : ISearchService
    {
        private readonly ISearchRepository _searchRepository;

        public SearchService(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        public async Task<SearchResults> SearchAllAsync(string query, string currentUserId, int limit = 10)
        {
            return await _searchRepository.SearchAllAsync(query, currentUserId, limit);
        }

        public async Task<List<Post>> GetRandomPostsAsync(int count = 10)
        {
            return await _searchRepository.GetRandomPostsAsync(count);
        }

        public async Task<List<Group>> GetRandomGroupsAsync(int count = 6)
        {
            return await _searchRepository.GetRandomGroupsAsync(count);
        }

        public async Task<List<IdentityUser>> GetRandomUsersToFollowAsync(string currentUserId, int count = 8)
        {
            return await _searchRepository.GetRandomUsersToFollowAsync(currentUserId, count);
        }
    }
}