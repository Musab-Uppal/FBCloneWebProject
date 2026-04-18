using Microsoft.AspNetCore.Identity;

namespace semproject.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; }

        // Navigation properties (will be populated manually by Dapper)
        public IdentityUser User { get; set; }
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

    }

    public class Like
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Post Post { get; set; }
        public IdentityUser User { get; set; }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int PostId { get; set; }
        public string UserId { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public IdentityUser User { get; set; }
    }
}