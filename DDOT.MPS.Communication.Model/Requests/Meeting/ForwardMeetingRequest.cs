using DDOT.MPS.Communication.Model.Dtos;

namespace DDOT.MPS.Communication.Model.Requests
{
    public class ForwardMeetingRequest
    {
        public string ReferanceId { get; set; }

        public string? Comment { get; set; }

        public List<EmailAddressDto> ToRecipients { get; set; }
    }
}
