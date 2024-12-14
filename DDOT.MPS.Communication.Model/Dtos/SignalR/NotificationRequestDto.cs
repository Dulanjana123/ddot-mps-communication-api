using DDOT.MPS.Communication.Core.Enums;

namespace DDOT.MPS.Communication.Model.Dtos
{
    public class NotificationRequestDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Email { get; set; }
        public NotificationType Type { get; set; }
    }
}
