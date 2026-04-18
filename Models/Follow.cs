using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace semproject.Models
{
    public class Follow
    {
        
        public int Id { get; set; }

      
        public string FollowerId { get; set; }  // User who is following

       
        public string FollowingId { get; set; } // User being followed

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        
        public virtual IdentityUser Follower { get; set; }

       
        public virtual IdentityUser Following { get; set; }
    }
}