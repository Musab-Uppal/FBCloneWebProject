using Dapper;
using System.Data;
using Microsoft.AspNetCore.Identity;
using semproject.Repositories.Interfaces;

namespace semproject.Repositories.Implementations
{
    public class FollowRepository : IFollowRepository
    {
        private readonly IDbConnection _db;

        public FollowRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
        {
            if (followerId == followingId)
                throw new InvalidOperationException("You cannot follow yourself");

            var checkQuery = @"
                SELECT COUNT(*) FROM Follows 
                WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId";

            var exists = await _db.ExecuteScalarAsync<int>(checkQuery, new
            {
                FollowerId = followerId,
                FollowingId = followingId
            });

            if (exists == 0)
            {
                var insertQuery = @"
                    INSERT INTO Follows (FollowerId, FollowingId, FollowedAt)
                    VALUES (@FollowerId, @FollowingId, @FollowedAt)";

                await _db.ExecuteAsync(insertQuery, new
                {
                    FollowerId = followerId,
                    FollowingId = followingId,
                    FollowedAt = DateTime.UtcNow
                });
                return true;
            }
            else
            {
                var deleteQuery = @"
                    DELETE FROM Follows 
                    WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId";

                await _db.ExecuteAsync(deleteQuery, new
                {
                    FollowerId = followerId,
                    FollowingId = followingId
                });
                return false;
            }
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followingId)
        {
            var query = @"
                SELECT COUNT(*) FROM Follows 
                WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId";

            return await _db.ExecuteScalarAsync<int>(query, new
            {
                FollowerId = followerId,
                FollowingId = followingId
            }) > 0;
        }

        public async Task<int> GetFollowerCountAsync(string userId)
        {
            var query = "SELECT COUNT(*) FROM Follows WHERE FollowingId = @UserId";
            return await _db.ExecuteScalarAsync<int>(query, new { UserId = userId });
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            var query = "SELECT COUNT(*) FROM Follows WHERE FollowerId = @UserId";
            return await _db.ExecuteScalarAsync<int>(query, new { UserId = userId });
        }

        public async Task<List<IdentityUser>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;

            var query = @"
                SELECT u.*
                FROM AspNetUsers u
                INNER JOIN Follows f ON u.Id = f.FollowerId
                WHERE f.FollowingId = @UserId
                ORDER BY f.FollowedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return (await _db.QueryAsync<IdentityUser>(query, new
            {
                UserId = userId,
                PageSize = pageSize,
                Offset = offset
            })).ToList();
        }

        public async Task<List<IdentityUser>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;

            var query = @"
                SELECT u.*
                FROM AspNetUsers u
                INNER JOIN Follows f ON u.Id = f.FollowingId
                WHERE f.FollowerId = @UserId
                ORDER BY f.FollowedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return (await _db.QueryAsync<IdentityUser>(query, new
            {
                UserId = userId,
                PageSize = pageSize,
                Offset = offset
            })).ToList();
        }

        public async Task<List<IdentityUser>> GetSuggestedUsersAsync(string userId, int limit = 10)
        {
            var followingQuery = "SELECT FollowingId FROM Follows WHERE FollowerId = @UserId";
            var followingIds = (await _db.QueryAsync<string>(followingQuery, new { UserId = userId })).ToList();

            var suggestedQuery = @"
                SELECT TOP(@Limit) f.FollowingId
                FROM Follows f
                WHERE f.FollowerId IN @FollowingIds 
                AND f.FollowingId NOT IN @FollowingIds
                AND f.FollowingId != @UserId
                GROUP BY f.FollowingId
                ORDER BY COUNT(*) DESC";

            var suggestedUserIds = (await _db.QueryAsync<string>(suggestedQuery, new
            {
                FollowingIds = followingIds.Any() ? followingIds : new List<string> { "" },
                UserId = userId,
                Limit = limit
            })).ToList();

            if (!suggestedUserIds.Any())
                return new List<IdentityUser>();

            var usersQuery = "SELECT * FROM AspNetUsers WHERE Id IN @SuggestedUserIds AND Id != @UserId";
            return (await _db.QueryAsync<IdentityUser>(usersQuery, new
            {
                SuggestedUserIds = suggestedUserIds,
                UserId = userId
            })).ToList();
        }

        public async Task<List<IdentityUser>> SearchUsersToFollowAsync(string currentUserId, string query)
        {
            var sql = @"
                SELECT * FROM AspNetUsers 
                WHERE Id != @CurrentUserId 
                AND (UserName LIKE @Query OR Email LIKE @Query)
                ORDER BY UserName";

            return (await _db.QueryAsync<IdentityUser>(sql, new
            {
                CurrentUserId = currentUserId,
                Query = $"%{query}%"
            })).ToList();
        }
    }
}