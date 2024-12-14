using DDOT.MPS.Communication.Core.Enums;

namespace DDOT.MPS.Communication.Model.Dtos
{
    public class GlobalNotificationRequestDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; }
    }
}
