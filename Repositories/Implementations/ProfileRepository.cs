using Dapper;
using System.Data;
using Microsoft.AspNetCore.Identity;
using semproject.Models;
using semproject.Repositories.Interfaces;

namespace semproject.Repositories.Implementations
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IDbConnection _db;

        public ProfileRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<UserProfile> GetUserProfileAsync(string userId, string currentUserId = null)
        {
            var userQuery = "SELECT * FROM AspNetUsers WHERE Id = @UserId";
            var user = await _db.QueryFirstOrDefaultAsync<IdentityUser>(userQuery, new { UserId = userId });

            if (user == null)
                return null;

            var profile = new UserProfile
            {
                User = user
            };

            profile.PostCount = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Posts WHERE UserId = @UserId",
                new { UserId = userId });

            profile.FollowerCount = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Follows WHERE FollowingId = @UserId",
                new { UserId = userId });

            profile.FollowingCount = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Follows WHERE FollowerId = @UserId",
                new { UserId = userId });

            profile.GroupCount = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM GroupMembers WHERE UserId = @UserId",
                new { UserId = userId });

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var followQuery = @"
                    SELECT COUNT(*) FROM Follows 
                    WHERE FollowerId = @CurrentUserId AND FollowingId = @UserId";

                profile.IsFollowing = await _db.ExecuteScalarAsync<int>(followQuery, new
                {
                    CurrentUserId = currentUserId,
                    UserId = userId
                }) > 0;
            }

            var postsQuery = @"
                SELECT TOP(20) p.*, 
                       (SELECT COUNT(*) FROM Likes WHERE PostId = p.Id) as LikeCount,
                       (SELECT COUNT(*) FROM Comments WHERE PostId = p.Id) as CommentCount
                FROM Posts p
                WHERE p.UserId = @UserId
                ORDER BY p.CreatedAt DESC";

            profile.Posts = (await _db.QueryAsync<Post>(postsQuery, new { UserId = userId })).ToList();

            var groupsQuery = @"
                SELECT TOP(10) g.*, u.*
                FROM Groups g
                INNER JOIN GroupMembers gm ON g.Id = gm.GroupId
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE gm.UserId = @UserId";

            var groupDictionary = new Dictionary<int, Group>();
            profile.Groups = (await _db.QueryAsync<Group, IdentityUser, Group>(
                groupsQuery,
                (group, createdBy) =>
                {
                    if (!groupDictionary.TryGetValue(group.Id, out var existingGroup))
                    {
                        existingGroup = group;
                        existingGroup.CreatedBy = createdBy;
                        groupDictionary.Add(existingGroup.Id, existingGroup);
                    }
                    return existingGroup;
                },
                new { UserId = userId },
                splitOn: "Id"
            )).Distinct().ToList();

            var followersQuery = @"
                SELECT TOP(10) u.*
                FROM AspNetUsers u
                INNER JOIN Follows f ON u.Id = f.FollowerId
                WHERE f.FollowingId = @UserId
                ORDER BY f.FollowedAt DESC";

            profile.Followers = (await _db.QueryAsync<IdentityUser>(followersQuery, new { UserId = userId })).ToList();

            var followingQuery = @"
                SELECT TOP(10) u.*
                FROM AspNetUsers u
                INNER JOIN Follows f ON u.Id = f.FollowingId
                WHERE f.FollowerId = @UserId
                ORDER BY f.FollowedAt DESC";

            profile.Following = (await _db.QueryAsync<IdentityUser>(followingQuery, new { UserId = userId })).ToList();

            return profile;
        }

        public async Task<UserStats> GetUserStatsAsync(string userId)
        {
            return new UserStats
            {
                PostCount = await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Posts WHERE UserId = @UserId",
                    new { UserId = userId }),

                FollowerCount = await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Follows WHERE FollowingId = @UserId",
                    new { UserId = userId }),

                FollowingCount = await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Follows WHERE FollowerId = @UserId",
                    new { UserId = userId }),

                GroupCount = await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM GroupMembers WHERE UserId = @UserId",
                    new { UserId = userId })
            };
        }
    }
}