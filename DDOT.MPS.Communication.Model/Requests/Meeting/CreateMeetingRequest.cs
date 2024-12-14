using DDOT.MPS.Communication.Model.Dtos;

namespace DDOT.MPS.Communication.Model.Requests
{
    public class CreateMeetingRequest
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string? Location { get; set; }

        public IList<AttendeeDtoBase> Attendees { get; set; }
    }
}
