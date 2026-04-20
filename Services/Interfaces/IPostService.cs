using semproject.Models;

namespace semproject.Services.Interfaces
{
    public interface IPostService
    {
        Task<Dictionary<int, bool>> GetUserLikesStatusAsync(string userId, List<int> postIds);
        Task<List<Post>> GetFollowedUsersPostsAsync(string currentUserId, int page = 1, int pageSize = 20);
        Task<List<Post>> GetRandomPostsAsync(int count = 10);
        Task<List<Post>> GetAllPostsAsync(int page = 1, int pageSize = 20);
        Task<Post> GetPostByIdAsync(int postId);
        Task<List<Post>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 20);
        Task<Post> CreatePostAsync(Post post, bool isGroupPost = false, int? groupId = null);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> UpdatePostContentAsync(int postId, string content, string userId);
        Task<bool> DeletePostAsync(int postId);
        Task<bool> LikePostAsync(string userId, int postId);
        Task<bool> UnlikePostAsync(string userId, int postId);
        Task<bool> ToggleLikeAsync(int postId, string userId);
        Task<int> GetLikeCountAsync(int postId);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<List<Comment>> GetPostCommentsAsync(int postId);
    }
}