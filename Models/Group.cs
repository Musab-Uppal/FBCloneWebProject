using Microsoft.AspNetCore.Identity;
using semproject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace semproject.Models
{
    public class Group
    {
      
        public int Id { get; set; }

        
        public string Name { get; set; }

       
        public string Description { get; set; }

        public string CreatedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPrivate { get; set; } = false;

        public string? CoverImageUrl { get; set; }

        [NotMapped]
        public int MemberCount { get; set; }

        
        public virtual IdentityUser CreatedBy { get; set; }

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<GroupPost> Posts { get; set; } = new List<GroupPost>();
    }

    public class GroupMember
    {
        public int Id { get; set; }

      
        public int GroupId { get; set; }

        
        public string UserId { get; set; }

        public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        
        public virtual Group Group { get; set; }

       
        public virtual IdentityUser User { get; set; }
    }

    public enum GroupMemberRole
    {
        Member,
        Moderator,
        Admin
    }

    public class GroupPost
    {
     
        public int Id { get; set; }

      
        public int GroupId { get; set; }

       
        public int PostId { get; set; }

        public DateTime SharedAt { get; set; } = DateTime.UtcNow;

       
        public virtual Group Group { get; set; }

        
        public virtual Post Post { get; set; }
    }
}