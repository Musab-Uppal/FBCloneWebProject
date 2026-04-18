using Dapper;
using semproject.Models;
using semproject.Repositories.Interfaces;
using System.Data;

namespace semproject.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbConnection _db;

        public NotificationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<Notification> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Notifications WHERE Id = @Id";
            return await _db.QueryFirstOrDefaultAsync<Notification>(query, new { Id = id });
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            var query = "SELECT * FROM Notifications ORDER BY CreatedAt DESC";
            return (await _db.QueryAsync<Notification>(query)).ToList();
        }

        public async Task<Notification> AddAsync(Notification entity)
        {
            var query = @"
                INSERT INTO Notifications (UserId, Message, Type, ActionUrl, RelatedUserId, CreatedAt, IsRead)
                VALUES (@UserId, @Message, @Type, @ActionUrl, @RelatedUserId, @CreatedAt, @IsRead);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            entity.Id = await _db.ExecuteScalarAsync<int>(query, entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(Notification entity)
        {
            var query = @"
                UPDATE Notifications 
                SET Message = @Message, Type = @Type, ActionUrl = @ActionUrl, IsRead = @IsRead
                WHERE Id = @Id";

            var result = await _db.ExecuteAsync(query, entity);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Notifications WHERE Id = @Id";
            var result = await _db.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Notifications";
            return await _db.ExecuteScalarAsync<int>(query);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 20)
        {
            var query = @"
                SELECT TOP(@Limit) * FROM Notifications 
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC";

            return (await _db.QueryAsync<Notification>(query, new { UserId = userId, Limit = limit })).ToList();
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var query = "SELECT COUNT(*) FROM Notifications WHERE UserId = @UserId AND IsRead = 0";
            return await _db.ExecuteScalarAsync<int>(query, new { UserId = userId });
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var query = "UPDATE Notifications SET IsRead = 1 WHERE Id = @Id";
            await _db.ExecuteAsync(query, new { Id = notificationId });
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var query = "UPDATE Notifications SET IsRead = 1 WHERE UserId = @UserId AND IsRead = 0";
            await _db.ExecuteAsync(query, new { UserId = userId });
        }

        public async Task<List<Notification>> GetNotificationsAsync(string userId, int page, int pageSize)
        {
            var query = @"
                SELECT * FROM Notifications 
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            return (await _db.QueryAsync<Notification>(query, new { UserId = userId, Offset = offset, PageSize = pageSize })).ToList();
        }

        public async Task<int> MarkAllAsReadAndGetCountAsync(string userId)
        {
            var query = "UPDATE Notifications SET IsRead = 1 WHERE UserId = @UserId AND IsRead = 0";
            return await _db.ExecuteAsync(query, new { UserId = userId });
        }
    }
}