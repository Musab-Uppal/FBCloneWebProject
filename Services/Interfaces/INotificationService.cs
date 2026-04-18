using semproject.Models;

namespace semproject.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateFollowNotificationAsync(string followerId, string followingId);
        Task CreateLikeNotificationAsync(string userId, string postOwnerId, int postId);
        Task CreateCommentNotificationAsync(string userId, string postOwnerId, int postId, string commentText);
        Task CreateGroupJoinNotificationAsync(string userId, string groupId, string groupName);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 20);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
        Task DeleteNotificationAsync(int notificationId);

        // Methods used by NotificationController
        Task<List<Notification>> GetNotificationsAsync(string userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<Notification>> GetLatestNotificationsAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<int> MarkAllAsReadAsync(string userId);
        Task<Notification> GetNotificationAsync(int notificationId, string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
    }
}