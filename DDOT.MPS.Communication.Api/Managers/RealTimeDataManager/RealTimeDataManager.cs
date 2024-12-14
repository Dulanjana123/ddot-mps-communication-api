using DDOT.MPS.Communication.Api.SignalrHubs;
using DDOT.MPS.Communication.Core.Enums;
using Microsoft.AspNetCore.SignalR;

namespace DDOT.MPS.Communication.Api.Managers
{
    public class RealTimeDataManager : IRealTimeDataManager
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        private static Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public RealTimeDataManager(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Notify(string email, NotificationType type, string title, string body)
        {
            // Notify only the client that matches the provided email
            await _hubContext.Clients.Group(email).SendAsync("ReceiveNotification", type.ToString(), title, body);
        }

        public async Task SendGlobalNotification(NotificationType type, string title, string body)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", type.ToString(), title, body);
        }

    }
}
