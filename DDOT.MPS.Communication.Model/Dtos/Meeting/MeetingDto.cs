namespace DDOT.MPS.Communication.Model.Dtos
{
    public class MeetingDto : MeetingBaseDto
    {
        public string? ReferanceId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }        
        public string? Subject { get; set; }
        public EmailAddressDto? Organizer { get; set; }
        public required IList<AttendeeDto> Attendees { get; set; }
        public required string WebLink { get; set; }
        public required string OnlineMeetingUrl { get; set; }
    }
}
