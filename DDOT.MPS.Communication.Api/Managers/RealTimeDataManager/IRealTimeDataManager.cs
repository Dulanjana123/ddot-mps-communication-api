using DDOT.MPS.Communication.Core.Enums;

namespace DDOT.MPS.Communication.Api.Managers
{
    public interface IRealTimeDataManager
    {
        public Task Notify(string email, NotificationType type, string title, string body);
        public Task SendGlobalNotification(NotificationType type, string title, string body);
    }
}
