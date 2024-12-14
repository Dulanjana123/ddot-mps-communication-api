using Microsoft.AspNetCore.SignalR;
using System.Web;

namespace DDOT.MPS.Communication.Api.SignalrHubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            string email = HttpUtility.UrlDecode(Context.GetHttpContext().Request.Query["email"]);
            _logger.LogInformation($"User with email {email} connected.");

            // Use email as the identifier for SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, email);
        }
    }
}
