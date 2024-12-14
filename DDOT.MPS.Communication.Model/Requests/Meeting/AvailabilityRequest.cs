using DDOT.MPS.Communication.Model.Dtos;

namespace DDOT.MPS.Communication.Model.Requests
{
    public class AvailabilityRequest
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public required List<string> Schedules { get; set; }
    }
}
