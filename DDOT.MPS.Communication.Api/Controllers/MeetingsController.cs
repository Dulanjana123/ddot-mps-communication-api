using DDOT.MPS.Communication.Api.Managers;
using DDOT.MPS.Communication.Model.Dtos;
using DDOT.MPS.Communication.Model.Requests;
using DDOT.MPS.Communication.Model.Responses;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

namespace DDOT.MPS.Communication.Api.Controllers
{
    [ApiController]
    public class MeetingsController : CoreController
    {
        private readonly ILogger<MeetingsController> _logger;
        private readonly IMeetingManager _meetingManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public MeetingsController(IMeetingManager meetingManager, ILogger<MeetingsController> logger)
        {
            _meetingManager = meetingManager ?? throw new ArgumentNullException(nameof(meetingManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("create")]
        public async Task<ActionResult<BaseResponse<MeetingDto>>> Create(CreateMeetingRequest meetingRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Controllers.MeetingsController.Create | Request in progress. | Meeting create request: {request}", JsonSerializer.Serialize(meetingRequest));
            return Ok(await _meetingManager.CreateMeeting(meetingRequest));
        }

        [HttpPost("availability")]
        public async Task<ActionResult<BaseResponse<IList<AvailabilityDto>>>> Availability(AvailabilityRequest availabilityRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Controllers.MeetingsController.Availability | Request in progress. | Meeting availability request: {request}", JsonSerializer.Serialize(availabilityRequest));
            return Ok(await _meetingManager.GetAvailability(availabilityRequest));
        }

        [HttpPost("forward")]
        public async Task<ActionResult<BaseResponse<bool>>> Forward(ForwardMeetingRequest forwardRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Controllers.MeetingsController.Forward | Request in progress. | Meeting create request: {request}", JsonSerializer.Serialize(forwardRequest));
            return Ok(await _meetingManager.ForwardMeeting(forwardRequest));
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<BaseResponse<bool>>> Cancel(CancelMeetingRequest cancelRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Controllers.MeetingsController.Cancel | Request in progress. | Meeting cancel request: {request}", JsonSerializer.Serialize(cancelRequest));
            return Ok(await _meetingManager.CancelMeeting(cancelRequest));
        }
    }
}
