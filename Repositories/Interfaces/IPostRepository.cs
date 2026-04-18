using semproject.Models;

namespace semproject.Repositories.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<List<Post>> GetFollowedUsersPostsAsync(string currentUserId, int page, int pageSize);
        Task<List<Post>> GetRandomPostsAsync(int count);
        Task<List<Post>> GetAllPostsAsync(int page, int pageSize);
        Task<List<Post>> GetUserPostsAsync(string userId, int page, int pageSize);
        Task<Dictionary<int, bool>> GetUserLikesStatusAsync(string userId, List<int> postIds);
        Task<bool> LikePostAsync(string userId, int postId);
        Task<bool> UnlikePostAsync(string userId, int postId);
        Task<int> GetLikeCountAsync(int postId);
        Task<List<Comment>> GetPostCommentsAsync(int postId);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
    }
}