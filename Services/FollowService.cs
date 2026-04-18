using Dapper;
using System.Data;
using Microsoft.AspNetCore.Identity;
using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;

        public FollowService(IFollowRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
        {
            return await _followRepository.ToggleFollowAsync(followerId, followingId);
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followingId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followingId);
        }

        public async Task<int> GetFollowerCountAsync(string userId)
        {
            return await _followRepository.GetFollowerCountAsync(userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _followRepository.GetFollowingCountAsync(userId);
        }

        public async Task<List<IdentityUser>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _followRepository.GetFollowersAsync(userId, page, pageSize);
        }

        public async Task<List<IdentityUser>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _followRepository.GetFollowingAsync(userId, page, pageSize);
        }

        public async Task<List<IdentityUser>> GetSuggestedUsersAsync(string userId, int limit = 10)
        {
            return await _followRepository.GetSuggestedUsersAsync(userId, limit);
        }

        public async Task<List<IdentityUser>> SearchUsersToFollowAsync(string currentUserId, string query)
        {
            return await _followRepository.SearchUsersToFollowAsync(currentUserId, query);
        }
    }
}