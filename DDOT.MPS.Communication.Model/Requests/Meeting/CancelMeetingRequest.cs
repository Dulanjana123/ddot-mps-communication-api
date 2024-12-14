using DDOT.MPS.Communication.Model.Dtos;

namespace DDOT.MPS.Communication.Model.Requests
{
    public class CancelMeetingRequest
    {
        public string ReferanceId { get; set; }

        public string? Comment { get; set; }        
    }
}
