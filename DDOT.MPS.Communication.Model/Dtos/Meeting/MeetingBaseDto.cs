using DDOT.MPS.Communication.Core.Constants;

namespace DDOT.MPS.Communication.Model.Dtos
{
    public class MeetingBaseDto
    {
        public string? Start { get; set; }
        public string? End { get; set; }
        public string StartEndTimeZone { get { return MeetingConstants.EasternStandardTimeZone; } }
    }
}
