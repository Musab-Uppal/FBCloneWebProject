using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace semproject.Models
{
    public class Notification
    {
        
        public int Id { get; set; }

        
        public string UserId { get; set; }  // User who receives the notification

        
       
        public string Message { get; set; }

      
        public string? ActionUrl { get; set; }  // URL for notification action

        public bool IsRead { get; set; } = false;

        public NotificationType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RelatedUserId { get; set; }  // User who triggered the notification

    
        public virtual IdentityUser User { get; set; }

    
        public virtual IdentityUser RelatedUser { get; set; }
    }

    public enum NotificationType
    {
        Follow,
        Like,
        Comment,
        GroupJoin,
        PostShare
    }
}