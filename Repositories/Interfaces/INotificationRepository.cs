using semproject.Models;

namespace semproject.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
        Task<List<Notification>> GetNotificationsAsync(string userId, int page, int pageSize);
        Task<int> MarkAllAsReadAndGetCountAsync(string userId);
    }
}