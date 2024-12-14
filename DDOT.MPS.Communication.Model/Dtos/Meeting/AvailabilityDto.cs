namespace DDOT.MPS.Communication.Model.Dtos
{
    public class AvailabilityDto
    {
        public string? ScheduleId { get; set; }

        public bool? Available { get; set; }

        public string? ErrorMessage { get; set; }

        public List<AvailabileMeeting>? Meetings { get; set; }
    }

    public class AvailabileMeeting : MeetingBaseDto
    {
        public string? Status { get; set; }
    }
}
