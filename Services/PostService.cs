using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IGroupRepository _groupRepository;

        public PostService(IPostRepository postRepository, IGroupRepository groupRepository)
        {
            _postRepository = postRepository;
            _groupRepository = groupRepository;
        }

        public async Task<Dictionary<int, bool>> GetUserLikesStatusAsync(string userId, List<int> postIds)
        {
            return await _postRepository.GetUserLikesStatusAsync(userId, postIds);
        }

        public async Task<List<Post>> GetFollowedUsersPostsAsync(string currentUserId, int page = 1, int pageSize = 20)
        {
            return await _postRepository.GetFollowedUsersPostsAsync(currentUserId, page, pageSize);
        }

        public async Task<List<Post>> GetRandomPostsAsync(int count = 10)
        {
            return await _postRepository.GetRandomPostsAsync(count);
        }

        public async Task<List<Post>> GetRandomPostsPageAsync(int page = 1, int pageSize = 10, int seed = 0)
        {
            return await _postRepository.GetRandomPostsPageAsync(page, pageSize, seed);
        }

        public async Task<List<Post>> GetAllPostsAsync(int page = 1, int pageSize = 20)
        {
            return await _postRepository.GetAllPostsAsync(page, pageSize);
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            return await _postRepository.GetByIdAsync(postId);
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _postRepository.GetUserPostsAsync(userId, page, pageSize);
        }

        public async Task<Post> CreatePostAsync(Post post, bool isGroupPost = false, int? groupId = null)
        {
            var createdPost = await _postRepository.AddAsync(post);

            if (isGroupPost)
            {
                if (!groupId.HasValue)
                {
                    throw new ArgumentException("A groupId is required when isGroupPost is true.", nameof(groupId));
                }

                try
                {
                    await _groupRepository.SharePostToGroupAsync(groupId.Value, createdPost.Id, createdPost.UserId);
                }
                catch
                {
                    // Keep data consistent: if group sharing fails, remove the post that was just created.
                    await _postRepository.DeleteAsync(createdPost.Id);
                    throw;
                }
            }

            return createdPost;
        }

        

        public async Task<bool> UpdatePostAsync(Post post)
        {
            return await _postRepository.UpdateAsync(post);
        }

        public async Task<bool> UpdatePostContentAsync(int postId, string content, string userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.UserId != userId)
                return false;

            post.Content = content;
            return await _postRepository.UpdateAsync(post);
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            return await _postRepository.DeleteAsync(postId);
        }

        public async Task<bool> LikePostAsync(string userId, int postId)
        {
            return await _postRepository.LikePostAsync(userId, postId);
        }

        public async Task<bool> UnlikePostAsync(string userId, int postId)
        {
            return await _postRepository.UnlikePostAsync(userId, postId);
        }

        public async Task<bool> ToggleLikeAsync(int postId, string userId)
        {
            var isLiked = await _postRepository.GetUserLikesStatusAsync(userId, new List<int> { postId });
            if (isLiked.ContainsKey(postId) && isLiked[postId])
                return !await _postRepository.UnlikePostAsync(userId, postId);
            else
                return await _postRepository.LikePostAsync(userId, postId);
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _postRepository.GetLikeCountAsync(postId);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            return await _postRepository.AddCommentAsync(comment);
        }

        

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _postRepository.DeleteCommentAsync(commentId);
        }

        public async Task<List<Comment>> GetPostCommentsAsync(int postId)
        {
            return await _postRepository.GetPostCommentsAsync(postId);
        }
    }
}