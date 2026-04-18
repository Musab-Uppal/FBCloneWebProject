using Dapper;
using Microsoft.AspNetCore.Identity;
using semproject.Models;
using semproject.Repositories.Interfaces;
using System.Data;

namespace semproject.Repositories.Implementations
{
    public class SearchRepository : ISearchRepository
    {
        private readonly IDbConnection _db;

        public SearchRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<SearchResults> SearchAllAsync(string query, string currentUserId, int limit = 10)
        {
            var results = new SearchResults();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            var userQuery = @"
                SELECT TOP(@Limit) * FROM AspNetUsers 
                WHERE Id != @CurrentUserId 
                AND (UserName LIKE @Query OR Email LIKE @Query)";

            results.Users = (await _db.QueryAsync<IdentityUser>(userQuery, new
            {
                CurrentUserId = currentUserId,
                Query = $"%{query}%",
                Limit = limit
            })).ToList();

            var postQuery = @"
                SELECT TOP(@Limit) p.*, u.*, 
                       (SELECT COUNT(*) FROM Likes WHERE PostId = p.Id) as LikeCount,
                       (SELECT COUNT(*) FROM Comments WHERE PostId = p.Id) as CommentCount
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                WHERE p.Content LIKE @Query
                ORDER BY p.CreatedAt DESC";

            var postDictionary = new Dictionary<int, Post>();
            results.Posts = (await _db.QueryAsync<Post, IdentityUser, Post>(
                postQuery,
                (post, user) =>
                {
                    if (!postDictionary.TryGetValue(post.Id, out var existingPost))
                    {
                        existingPost = post;
                        existingPost.User = user;
                        existingPost.Likes = new List<Like>();
                        existingPost.Comments = new List<Comment>();
                        postDictionary.Add(existingPost.Id, existingPost);
                    }
                    return existingPost;
                },
                new { Query = $"%{query}%", Limit = limit },
                splitOn: "Id"
            )).Distinct().ToList();

            var groupQuery = @"
                SELECT TOP(@Limit) g.*, 
                       u.*,
                       (SELECT COUNT(*) FROM GroupMembers WHERE GroupId = g.Id) as MemberCount
                FROM Groups g
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE (g.Name LIKE @Query OR g.Description LIKE @Query)
                AND g.IsPrivate = 0";

            var groupDictionary = new Dictionary<int, Group>();
            results.Groups = (await _db.QueryAsync<Group, IdentityUser, Group>(
                groupQuery,
                (group, createdBy) =>
                {
                    if (!groupDictionary.TryGetValue(group.Id, out var existingGroup))
                    {
                        existingGroup = group;
                        existingGroup.CreatedBy = createdBy;
                        existingGroup.Members = new List<GroupMember>();
                        groupDictionary.Add(existingGroup.Id, existingGroup);
                    }
                    return existingGroup;
                },
                new { Query = $"%{query}%", Limit = limit },
                splitOn: "Id"
            )).Distinct().ToList();

            return results;
        }

        public async Task<List<Post>> GetRandomPostsAsync(int count = 10)
        {
            var query = @"
                SELECT TOP(@Count) p.*, u.*
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                ORDER BY NEWID()";

            var postDictionary = new Dictionary<int, Post>();
            return (await _db.QueryAsync<Post, IdentityUser, Post>(
                query,
                (post, user) =>
                {
                    if (!postDictionary.TryGetValue(post.Id, out var existingPost))
                    {
                        existingPost = post;
                        existingPost.User = user;
                        postDictionary.Add(existingPost.Id, existingPost);
                    }
                    return existingPost;
                },
                new { Count = count },
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<List<Group>> GetRandomGroupsAsync(int count = 6)
        {
            var query = @"
                SELECT TOP(@Count) g.*, u.*
                FROM Groups g
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE g.IsPrivate = 0
                ORDER BY NEWID()";

            var groupDictionary = new Dictionary<int, Group>();
            return (await _db.QueryAsync<Group, IdentityUser, Group>(
                query,
                (group, createdBy) =>
                {
                    if (!groupDictionary.TryGetValue(group.Id, out var existingGroup))
                    {
                        existingGroup = group;
                        existingGroup.CreatedBy = createdBy;
                        existingGroup.Members = new List<GroupMember>();
                        groupDictionary.Add(existingGroup.Id, existingGroup);
                    }
                    return existingGroup;
                },
                new { Count = count },
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<List<IdentityUser>> GetRandomUsersToFollowAsync(string currentUserId, int count = 8)
        {
            var query = @"
                SELECT TOP(@Count) u.* 
                FROM AspNetUsers u
                WHERE u.Id != @CurrentUserId
                AND u.Id NOT IN (SELECT FollowingId FROM Follows WHERE FollowerId = @CurrentUserId)
                ORDER BY NEWID()";

            return (await _db.QueryAsync<IdentityUser>(query, new { CurrentUserId = currentUserId, Count = count })).ToList();
        }
    }
}