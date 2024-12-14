using DDOT.MPS.Communication.Model.Dtos;
using DDOT.MPS.Communication.Model.Requests;
using DDOT.MPS.Communication.Model.Responses;

namespace DDOT.MPS.Communication.Api.Managers
{
    public interface IMeetingManager
    {
        Task<BaseResponse<bool>> CancelMeeting(CancelMeetingRequest cancelRequest);
        Task<BaseResponse<MeetingDto>> CreateMeeting(CreateMeetingRequest meetingRequest);
        Task<BaseResponse<bool>> ForwardMeeting(ForwardMeetingRequest forwardRequest);
        Task<BaseResponse<IList<AvailabilityDto>>> GetAvailability(AvailabilityRequest availabilityRequest);
    }
}
