using Microsoft.AspNetCore.Identity;

namespace semproject.Models
{
    public class SearchResults
    {
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Group> Groups { get; set; } = new List<Group>();
    }
}