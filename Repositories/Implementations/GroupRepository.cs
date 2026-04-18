using Dapper;
using Microsoft.AspNetCore.Identity;
using semproject.Models;
using semproject.Repositories.Interfaces;
using System.Data;

namespace semproject.Repositories.Implementations
{
    public class GroupRepository : IGroupRepository
    {
        private readonly IDbConnection _db;

        public GroupRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<Group> GetByIdAsync(int id)
        {
            var query = @"
                SELECT g.*, u.*
                FROM Groups g
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE g.Id = @Id";

            var groupDictionary = new Dictionary<int, Group>();
            var group = (await _db.QueryAsync<Group, IdentityUser, Group>(
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
                new { Id = id },
                splitOn: "Id"
            )).FirstOrDefault();

            if (group != null)
            {
                var membersQuery = @"
                    SELECT gm.*, u.*
                    FROM GroupMembers gm
                    LEFT JOIN AspNetUsers u ON gm.UserId = u.Id
                    WHERE gm.GroupId = @GroupId";

                var memberDictionary = new Dictionary<string, GroupMember>();
                group.Members = (await _db.QueryAsync<GroupMember, IdentityUser, GroupMember>(
                    membersQuery,
                    (member, user) =>
                    {
                        member.User = user;
                        return member;
                    },
                    new { GroupId = id },
                    splitOn: "Id"
                )).ToList();
            }

            return group;
        }

        public async Task<List<Group>> GetAllAsync()
        {
            var query = @"
                SELECT g.*, u.*
                FROM Groups g
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE g.IsPrivate = 0";

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
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<Group> AddAsync(Group entity)
        {
            var query = @"
                INSERT INTO Groups (Name, Description, CreatedById, IsPrivate, CreatedAt)
                VALUES (@Name, @Description, @CreatedById, @IsPrivate, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            entity.Id = await _db.ExecuteScalarAsync<int>(query, entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(Group entity)
        {
            var query = @"
                UPDATE Groups 
                SET Name = @Name, Description = @Description, IsPrivate = @IsPrivate, CoverImageUrl = @CoverImageUrl
                WHERE Id = @Id";

            var result = await _db.ExecuteAsync(query, entity);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Groups WHERE Id = @Id";
            var result = await _db.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Groups";
            return await _db.ExecuteScalarAsync<int>(query);
        }

        public async Task<List<Group>> SearchGroupsAsync(string searchTerm)
        {
            var query = @"
                SELECT TOP(20) g.*, u.*,
                       (SELECT COUNT(*) FROM GroupMembers WHERE GroupId = g.Id) as MemberCount
                FROM Groups g
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE g.IsPrivate = 0
                AND (g.Name LIKE @SearchTerm OR g.Description LIKE @SearchTerm)
                ORDER BY (SELECT COUNT(*) FROM GroupMembers WHERE GroupId = g.Id) DESC";

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
                new { SearchTerm = $"%{searchTerm}%" },
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<List<Group>> GetUserGroupsAsync(string userId)
        {
            var query = @"
                SELECT g.*, u.*
                FROM Groups g
                INNER JOIN GroupMembers gm ON g.Id = gm.GroupId
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE gm.UserId = @UserId";

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
                new { UserId = userId },
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<List<Group>> GetPublicGroupsAsync()
        {
            var query = @"
                SELECT g.*, u.*,
                       (SELECT COUNT(*) FROM GroupMembers WHERE GroupId = g.Id) as MemberCount
                FROM Groups g
                LEFT JOIN AspNetUsers u ON g.CreatedById = u.Id
                WHERE g.IsPrivate = 0
                ORDER BY g.CreatedAt DESC";

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
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<Group> GetGroupWithDetailsAsync(int groupId)
        {
            var group = await GetByIdAsync(groupId);

            if (group == null)
                return null;

            // Get group posts
            var postsQuery = @"
                SELECT gp.*, p.*, u.*
                FROM GroupPosts gp
                INNER JOIN Posts p ON gp.PostId = p.Id
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                WHERE gp.GroupId = @GroupId
                ORDER BY gp.SharedAt DESC";

            var groupPostDictionary = new Dictionary<int, GroupPost>();
            var groupPosts = await _db.QueryAsync<GroupPost, Post, IdentityUser, GroupPost>(
                postsQuery,
                (groupPost, post, user) =>
                {
                    if (!groupPostDictionary.TryGetValue(groupPost.Id, out var existingGroupPost))
                    {
                        existingGroupPost = groupPost;
                        existingGroupPost.Post = post;
                        if (post != null)
                        {
                            post.User = user;
                            post.Comments = new List<Comment>();
                            post.Likes = new List<Like>();
                        }
                        groupPostDictionary.Add(existingGroupPost.Id, existingGroupPost);
                    }
                    return existingGroupPost;
                },
                new { GroupId = groupId },
                splitOn: "Id"
            );

            // Load comments and likes for each post
            foreach (var groupPost in groupPosts)
            {
                if (groupPost.Post != null)
                {
                    // Load comments
                    var commentsQuery = @"
                        SELECT c.*, u.*
                        FROM Comments c
                        LEFT JOIN AspNetUsers u ON c.UserId = u.Id
                        WHERE c.PostId = @PostId
                        ORDER BY c.CreatedAt DESC";

                    var comments = await _db.QueryAsync<Comment, IdentityUser, Comment>(
                        commentsQuery,
                        (comment, commentUser) =>
                        {
                            comment.User = commentUser;
                            return comment;
                        },
                        new { PostId = groupPost.Post.Id },
                        splitOn: "Id"
                    );

                    groupPost.Post.Comments = comments.ToList();

                    // Load likes
                    var likesQuery = @"
                        SELECT l.*, u.*
                        FROM Likes l
                        LEFT JOIN AspNetUsers u ON l.UserId = u.Id
                        WHERE l.PostId = @PostId";

                    var likes = await _db.QueryAsync<Like, IdentityUser, Like>(
                        likesQuery,
                        (like, likeUser) =>
                        {
                            like.User = likeUser;
                            return like;
                        },
                        new { PostId = groupPost.Post.Id },
                        splitOn: "Id"
                    );

                    groupPost.Post.Likes = likes.ToList();
                }
            }

            group.Posts = groupPosts.ToList();
            return group;
        }

        public async Task<List<GroupMember>> GetGroupMembersAsync(int groupId)
        {
            var query = @"
                SELECT gm.*, u.*
                FROM GroupMembers gm
                LEFT JOIN AspNetUsers u ON gm.UserId = u.Id
                WHERE gm.GroupId = @GroupId";

            return (await _db.QueryAsync<GroupMember, IdentityUser, GroupMember>(
                query,
                (member, user) =>
                {
                    member.User = user;
                    return member;
                },
                new { GroupId = groupId },
                splitOn: "Id"
            )).ToList();
        }

        public async Task<bool> AddMemberToGroupAsync(int groupId, string userId)
        {
            var checkQuery = "SELECT COUNT(*) FROM GroupMembers WHERE GroupId = @GroupId AND UserId = @UserId";
            var exists = await _db.ExecuteScalarAsync<int>(checkQuery, new { GroupId = groupId, UserId = userId });

            if (exists == 0)
            {
                var insertQuery = @"
                    INSERT INTO GroupMembers (GroupId, UserId, JoinedAt)
                    VALUES (@GroupId, @UserId, @JoinedAt)";

                await _db.ExecuteAsync(insertQuery, new
                {
                    GroupId = groupId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                });
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveMemberFromGroupAsync(int groupId, string userId)
        {
            var query = "DELETE FROM GroupMembers WHERE GroupId = @GroupId AND UserId = @UserId";
            var result = await _db.ExecuteAsync(query, new { GroupId = groupId, UserId = userId });
            return result > 0;
        }

        public async Task<bool> IsUserGroupMemberAsync(int groupId, string userId)
        {
            var query = "SELECT COUNT(*) FROM GroupMembers WHERE GroupId = @GroupId AND UserId = @UserId";
            return await _db.ExecuteScalarAsync<int>(query, new { GroupId = groupId, UserId = userId }) > 0;
        }

        public async Task<bool> IsUserAdminAsync(int groupId, string userId)
        {
            var query = @"
                SELECT COUNT(*) FROM Groups 
                WHERE Id = @GroupId AND CreatedById = @UserId";
            return await _db.ExecuteScalarAsync<int>(query, new { GroupId = groupId, UserId = userId }) > 0;
        }

        public async Task<GroupPost> SharePostToGroupAsync(int groupId, int postId, string userId)
        {
            var isMember = await IsUserGroupMemberAsync(groupId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You must be a member of the group to share posts");

            var checkQuery = "SELECT COUNT(*) FROM GroupPosts WHERE GroupId = @GroupId AND PostId = @PostId";
            var exists = await _db.ExecuteScalarAsync<int>(checkQuery, new { GroupId = groupId, PostId = postId });

            if (exists > 0)
                throw new InvalidOperationException("This post is already shared in this group");

            var insertQuery = @"
                INSERT INTO GroupPosts (GroupId, PostId, SharedBy, SharedAt)
                VALUES (@GroupId, @PostId, @SharedBy, @SharedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await _db.ExecuteScalarAsync<int>(insertQuery, new
            {
                GroupId = groupId,
                PostId = postId,
                SharedBy = userId,
                SharedAt = DateTime.UtcNow
            });

            return new GroupPost { Id = id, GroupId = groupId, PostId = postId, SharedAt = DateTime.UtcNow };
        }
    }
}