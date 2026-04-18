using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using semproject.Hubs;
using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext,
            UserManager<IdentityUser> userManager)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task CreateFollowNotificationAsync(string followerId, string followingId)
        {
            var follower = await _userManager.FindByIdAsync(followerId);
            var followingUser = await _userManager.FindByIdAsync(followingId);

            if (follower == null || followingUser == null || followerId == followingId)
                return;

            var notification = new Notification
            {
                UserId = followingId,
                Message = $"{follower.UserName} started following you",
                Type = NotificationType.Follow,
                ActionUrl = $"/Profile/View/{followerId}",
                RelatedUserId = followerId,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.AddAsync(notification);
            await SendRealTimeNotificationAsync(createdNotification);
        }

        public async Task CreateLikeNotificationAsync(string userId, string postOwnerId, int postId)
        {
            if (userId == postOwnerId) return;

            var user = await _userManager.FindByIdAsync(userId);
            var postOwner = await _userManager.FindByIdAsync(postOwnerId);

            if (user == null || postOwner == null) return;

            var notification = new Notification
            {
                UserId = postOwnerId,
                Message = $"{user.UserName} liked your post",
                Type = NotificationType.Like,
                ActionUrl = $"/Home/Feed#post-{postId}",
                RelatedUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.AddAsync(notification);
            await SendRealTimeNotificationAsync(createdNotification);
        }

        public async Task CreateCommentNotificationAsync(string userId, string postOwnerId, int postId, string commentText)
        {
            if (userId == postOwnerId) return;

            var user = await _userManager.FindByIdAsync(userId);
            var postOwner = await _userManager.FindByIdAsync(postOwnerId);

            if (user == null || postOwner == null) return;

            var displayComment = commentText.Length > 30
                ? commentText.Substring(0, 27) + "..."
                : commentText;

            var notification = new Notification
            {
                UserId = postOwnerId,
                Message = $"{user.UserName} commented: \"{displayComment}\"",
                Type = NotificationType.Comment,
                ActionUrl = $"/Home/Feed#post-{postId}",
                RelatedUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.AddAsync(notification);
            await SendRealTimeNotificationAsync(createdNotification);
        }

        public async Task CreateGroupJoinNotificationAsync(string userId, string groupId, string groupName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return;

            var notification = new Notification
            {
                UserId = userId,
                Message = $"You joined the group: {groupName}",
                Type = NotificationType.GroupJoin,
                ActionUrl = $"/Groups/Details/{groupId}",
                RelatedUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsRead = true
            };

            await _notificationRepository.AddAsync(notification);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 20)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId, limit);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadNotificationCountAsync(userId);
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkNotificationAsReadAsync(notificationId);
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            await _notificationRepository.DeleteAsync(notificationId);
        }

        // Methods used by NotificationController

        public async Task<List<Notification>> GetNotificationsAsync(string userId, int page, int pageSize)
        {
            return await _notificationRepository.GetNotificationsAsync(userId, page, pageSize);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadNotificationCountAsync(userId);
        }

        public async Task<List<Notification>> GetLatestNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId, 10);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
                return false;

            await _notificationRepository.MarkNotificationAsReadAsync(notificationId);
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            return await _notificationRepository.MarkAllAsReadAndGetCountAsync(userId);
        }

        public async Task<Notification> GetNotificationAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
                return null;
            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
                return false;

            return await _notificationRepository.DeleteAsync(notificationId);
        }

        private async Task SendRealTimeNotificationAsync(Notification notification)
        {
            await _hubContext.Clients.Group($"user-{notification.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    timestamp = notification.CreatedAt.ToString("MMM dd, hh:mm tt"),
                    actionUrl = notification.ActionUrl
                });
        }
    }
}