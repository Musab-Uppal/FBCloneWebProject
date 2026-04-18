using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace semproject.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinNotificationGroup()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                await Clients.Caller.SendAsync("JoinedGroup", $"Connected to notifications for user {userId}");
            }
        }

        public async Task LeaveNotificationGroup()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
        }

        // Send notification to specific user
        public async Task SendNotificationToUser(string userId, string message, string type)
        {
            await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                message = message,
                type = type,
                timestamp = DateTime.UtcNow.ToString("MMM dd, hh:mm tt")
            });
        }

        public override async Task OnConnectedAsync()
        {
            await JoinNotificationGroup();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await LeaveNotificationGroup();
            await base.OnDisconnectedAsync(exception);
        }
    }
}