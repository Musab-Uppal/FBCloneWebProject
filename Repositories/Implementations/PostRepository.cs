using Dapper;
using Microsoft.AspNetCore.Identity;
using semproject.Models;
using semproject.Repositories.Interfaces;
using System.Data;

namespace semproject.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnection _db;

        public PostRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            var query = @"
                SELECT p.*, u.*
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                WHERE p.Id = @Id";

            var postDictionary = new Dictionary<int, Post>();
            var post = (await _db.QueryAsync<Post, IdentityUser, Post>(
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
                new { Id = id },
                splitOn: "Id"
            )).FirstOrDefault();

            if (post != null)
            {
                var likesQuery = "SELECT * FROM Likes WHERE PostId = @PostId";
                post.Likes = (await _db.QueryAsync<Like>(likesQuery, new { PostId = id })).ToList();

                var commentsQuery = @"
                    SELECT c.*, u.*
                    FROM Comments c
                    LEFT JOIN AspNetUsers u ON c.UserId = u.Id
                    WHERE c.PostId = @PostId
                    ORDER BY c.CreatedAt";

                var commentDictionary = new Dictionary<int, Comment>();
                post.Comments = (await _db.QueryAsync<Comment, IdentityUser, Comment>(
                    commentsQuery,
                    (comment, user) =>
                    {
                        comment.User = user;
                        return comment;
                    },
                    new { PostId = id },
                    splitOn: "Id"
                )).ToList();
            }

            return post;
        }

        public async Task<List<Post>> GetAllAsync()
        {
            var query = @"
                SELECT p.*, u.*
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                ORDER BY p.CreatedAt DESC";

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
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<Post> AddAsync(Post entity)
        {
            var query = @"
                INSERT INTO Posts (UserId, Content, CreatedAt, ImageUrl)
                VALUES (@UserId, @Content, @CreatedAt, @ImageUrl);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            entity.Id = await _db.ExecuteScalarAsync<int>(query, entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(Post entity)
        {
            var query = @"
                UPDATE Posts 
                SET Content = @Content, ImageUrl = @ImageUrl
                WHERE Id = @Id";

            var result = await _db.ExecuteAsync(query, entity);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Posts WHERE Id = @Id";
            var result = await _db.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Posts";
            return await _db.ExecuteScalarAsync<int>(query);
        }

        public async Task<List<Post>> GetFollowedUsersPostsAsync(string currentUserId, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;

            var query = @"
                SELECT p.*, u.*, 
                       (SELECT COUNT(*) FROM Likes WHERE PostId = p.Id) as LikeCount,
                       (SELECT COUNT(*) FROM Comments WHERE PostId = p.Id) as CommentCount
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                WHERE p.UserId IN (
                    SELECT FollowingId FROM Follows WHERE FollowerId = @CurrentUserId
                    UNION
                    SELECT @CurrentUserId
                )
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var postDictionary = new Dictionary<int, Post>();
            var posts = (await _db.QueryAsync<Post, IdentityUser, Post>(
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
                new { CurrentUserId = currentUserId, PageSize = pageSize, Offset = offset },
                splitOn: "Id"
            )).Distinct().ToList();

            if (posts.Any())
            {
                var postIds = posts.Select(p => p.Id).ToList();
                var commentsQuery = @"
                    SELECT c.*, u.*
                    FROM Comments c
                    LEFT JOIN AspNetUsers u ON c.UserId = u.Id
                    WHERE c.PostId IN @PostIds
                    ORDER BY c.CreatedAt";

                var comments = (await _db.QueryAsync<Comment, IdentityUser, Comment>(
                    commentsQuery,
                    (comment, user) =>
                    {
                        comment.User = user;
                        return comment;
                    },
                    new { PostIds = postIds },
                    splitOn: "Id"
                )).ToList();

                var commentsByPost = comments.GroupBy(c => c.PostId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var post in posts)
                {
                    if (commentsByPost.TryGetValue(post.Id, out var postComments))
                    {
                        post.Comments = postComments;
                    }
                }
            }

            return posts;
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

        public async Task<List<Post>> GetAllPostsAsync(int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;

            var query = @"
                SELECT p.*, u.*
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                new { PageSize = pageSize, Offset = offset },
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;

            var query = @"
                SELECT p.*, u.*
                FROM Posts p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                WHERE p.UserId = @UserId
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                new { UserId = userId, PageSize = pageSize, Offset = offset },
                splitOn: "Id"
            )).Distinct().ToList();
        }

        public async Task<Dictionary<int, bool>> GetUserLikesStatusAsync(string userId, List<int> postIds)
        {
            if (postIds == null || !postIds.Any())
                return new Dictionary<int, bool>();

            var query = @"
                SELECT PostId FROM Likes 
                WHERE UserId = @UserId AND PostId IN @PostIds";

            var userLikes = (await _db.QueryAsync<int>(query, new
            {
                UserId = userId,
                PostIds = postIds
            })).ToList();

            return postIds.ToDictionary(
                id => id,
                id => userLikes.Contains(id)
            );
        }

        public async Task<bool> LikePostAsync(string userId, int postId)
        {
            var checkQuery = "SELECT COUNT(*) FROM Likes WHERE PostId = @PostId AND UserId = @UserId";
            var exists = await _db.ExecuteScalarAsync<int>(checkQuery, new { PostId = postId, UserId = userId });

            if (exists == 0)
            {
                var insertQuery = @"
                    INSERT INTO Likes (PostId, UserId, LikedAt)
                    VALUES (@PostId, @UserId, @LikedAt)";

                await _db.ExecuteAsync(insertQuery, new
                {
                    PostId = postId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                });
                return true;
            }
            return false;
        }

        public async Task<bool> UnlikePostAsync(string userId, int postId)
        {
            var query = "DELETE FROM Likes WHERE PostId = @PostId AND UserId = @UserId";
            var result = await _db.ExecuteAsync(query, new { PostId = postId, UserId = userId });
            return result > 0;
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            var query = "SELECT COUNT(*) FROM Likes WHERE PostId = @PostId";
            return await _db.ExecuteScalarAsync<int>(query, new { PostId = postId });
        }

        public async Task<List<Comment>> GetPostCommentsAsync(int postId)
        {
            var query = @"
                SELECT c.*, u.*
                FROM Comments c
                LEFT JOIN AspNetUsers u ON c.UserId = u.Id
                WHERE c.PostId = @PostId
                ORDER BY c.CreatedAt";

            var commentDictionary = new Dictionary<int, Comment>();
            return (await _db.QueryAsync<Comment, IdentityUser, Comment>(
                query,
                (comment, user) =>
                {
                    comment.User = user;
                    return comment;
                },
                new { PostId = postId },
                splitOn: "Id"
            )).ToList();
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            var query = @"
                INSERT INTO Comments (PostId, UserId, Content, CreatedAt)
                VALUES (@PostId, @UserId, @Content, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            comment.Id = await _db.ExecuteScalarAsync<int>(query, comment);
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var query = "DELETE FROM Comments WHERE Id = @Id";
            var result = await _db.ExecuteAsync(query, new { Id = commentId });
            return result > 0;
        }
    }
}