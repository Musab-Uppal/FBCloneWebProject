using Microsoft.AspNetCore.Identity;
using semproject.Models;

namespace semproject.Models
{
    public class UserProfile
    {
        public IdentityUser User { get; set; }
        public int PostCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int GroupCount { get; set; }
        public bool IsFollowing { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<IdentityUser> Followers { get; set; } = new List<IdentityUser>();
        public List<IdentityUser> Following { get; set; } = new List<IdentityUser>();
    }

    public class UserStats
    {
        public int PostCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int GroupCount { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}

