using Microsoft.AspNetCore.Identity;

namespace semproject.Models
{
    public class DiscoverViewModel
    {
        public List<Post> RandomPosts { get; set; }
        public List<Group> RandomGroups { get; set; }
        public List<IdentityUser> SuggestedUsers { get; set; }
    }
}
