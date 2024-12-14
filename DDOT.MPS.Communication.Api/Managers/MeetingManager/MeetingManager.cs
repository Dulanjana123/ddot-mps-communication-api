using DDOT.MPS.Communication.Core.Constants;
using DDOT.MPS.Communication.Model.Configurations;
using DDOT.MPS.Communication.Model.Dtos;
using DDOT.MPS.Communication.Model.Requests;
using DDOT.MPS.Communication.Model.Responses;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Calendar.GetSchedule;
using Microsoft.Graph.Users.Item.Events.Item.Forward;
using Microsoft.Graph.Users.Item.Events.Item.Cancel;
using System.Text.Json;
using static DDOT.MPS.Communication.Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Communication.Api.Managers
{
    public class MeetingManager : IMeetingManager
    {
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<MeetingManager> _logger;
        private readonly string _organizerEmail;

        #region Public Methods

        public MeetingManager(IGraphClientService graphClientService, IOptions<DdotMpsMeetingConfigeration> ddotMpsGraphApiConfigerationOptions, ILogger<MeetingManager> logger)
        {
            _graphClient = graphClientService != null
                ? graphClientService.CreateGraphClient()
                : throw new ArgumentNullException(nameof(graphClientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _organizerEmail = ddotMpsGraphApiConfigerationOptions.Value.OrganizerEmail;
            if (string.IsNullOrWhiteSpace(_organizerEmail))
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager | Organizer email is null or empty.");
                throw new UDNotFoundException("PERMIT_MEETING_ORGANIZER_EMAIL_IS_NULL_OR_EMPTY");
            }
        }

        public async Task<BaseResponse<MeetingDto>> CreateMeeting(CreateMeetingRequest meetingRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Managers.MeetingManager.CreateMeeting | Create meeting request is inprogress.");
            string easternStandardTimeZone = MeetingConstants.EasternStandardTimeZone;
            string errorMssageCode = ValidateMeetingRequestDto(meetingRequest, easternStandardTimeZone);
            if (!string.IsNullOrWhiteSpace(errorMssageCode)) return new BaseResponse<MeetingDto> { Message = errorMssageCode, Success = false };

            //TODO createMeetingRequest.Organizer.Address or object id 
            Event? meetingResult = await _graphClient.Users[_organizerEmail].Events
                .PostAsync(GetEventByMeetingRequestDto(meetingRequest, easternStandardTimeZone), (requestConfiguration) =>
            {
                requestConfiguration.Headers.Add("Prefer", $"outlook.timezone=\"{easternStandardTimeZone}\"");
            });

            MeetingDto? meetingDto = GetMeetingDtoByGraphEvent(meetingResult);
            if (meetingDto == null) return new BaseResponse<MeetingDto> { Success = false, Message = "PERMIT_MEETING_CREATE_MEETING_REQUEST_FAIL" };

            _logger.LogInformation("DDOT.MPS.Communication.Api.Managers.MeetingManager.CreateMeeting | Create meeting created. | Meeting result: {meetingResult}", JsonSerializer.Serialize(meetingResult));
            return new BaseResponse<MeetingDto>
            {
                Success = true,
                Message = "PERMIT_MEETING_CREATE_MEETING_REQUEST_SUCCESS",
                Data = meetingDto
            };
        }

        public async Task<BaseResponse<IList<AvailabilityDto>>> GetAvailability(AvailabilityRequest availabilityRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Managers.MeetingManager.GetAvailability | Get availability request is inprogress.");

            string errorMssageCode = ValidateAvailabilityRequestDto(availabilityRequest);
            if (!string.IsNullOrWhiteSpace(errorMssageCode)) return new BaseResponse<IList<AvailabilityDto>> { Message = errorMssageCode, Success = false };

            DateTime scheduleStartDateTime = availabilityRequest.Start;
            DateTime scheduleEndDateTime = availabilityRequest.End;
            GetSchedulePostRequestBody requestBody = new GetSchedulePostRequestBody
            {
                Schedules = availabilityRequest.Schedules,
                StartTime = new DateTimeTimeZone
                {
                    DateTime = scheduleStartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = MeetingConstants.EasternStandardTimeZone,
                },
                EndTime = new DateTimeTimeZone
                {
                    DateTime = scheduleEndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = MeetingConstants.EasternStandardTimeZone,
                },
                AvailabilityViewInterval = (int)(scheduleEndDateTime - scheduleStartDateTime).TotalMinutes
            };

            GetSchedulePostResponse? schedulePostResponse = await _graphClient.Users[_organizerEmail].Calendar.GetSchedule
                .PostAsGetSchedulePostResponseAsync(requestBody, (requestConfiguration) =>
                {
                    requestConfiguration.Headers.Add("Prefer", $"outlook.timezone=\"{MeetingConstants.EasternStandardTimeZone}\"");
                });

            IList<AvailabilityDto>? availabilityDtoList = GetAvailabilityDtoListByGraphSchedulePostResponse(schedulePostResponse);
            if (availabilityDtoList == null || !availabilityDtoList.Any()) return new BaseResponse<IList<AvailabilityDto>> { Success = false, Message = "PERMIT_MEETING_AVAILABILITY_REQUEST_FAIL" };

            return new BaseResponse<IList<AvailabilityDto>>
            {
                Data = availabilityDtoList,
                Message = "PERMIT_MEETING_AVAILABILITY_REQUEST_SUCCESS",
                Success = true
            };
        }

        public async Task<BaseResponse<bool>> ForwardMeeting(ForwardMeetingRequest forwardRequest)
        {
            _logger.LogInformation("DDOT.MPS.Communication.Api.Managers.MeetingManager.ForwardMeeting | Forward meeting request is inprogress.");
            string errorMssageCode = ValidateForwardRequestDto(forwardRequest);
            if (!string.IsNullOrWhiteSpace(errorMssageCode)) return new BaseResponse<bool> { Message = errorMssageCode, Success = false, Data = false };

            await _graphClient.Users[_organizerEmail].Events[forwardRequest.ReferanceId].Forward.PostAsync(new ForwardPostRequestBody
            {
                ToRecipients = forwardRequest.ToRecipients.Select(r => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = r.Address,
                        Name = r.Name
                    }
                }).ToList(),
                Comment = forwardRequest.Comment,
            });

            return new BaseResponse<bool> { Success = true, Message = "PERMIT_MEETING_FORWARD_MEETING_REQUEST_SUCCESS", Data =true };
        }

        public async Task<BaseResponse<bool>> CancelMeeting(CancelMeetingRequest cancelRequest)
        {
            if (cancelRequest == null)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.CancelMeeting | Cancel request is null.");
                throw new UDArgumentException("PERMIT_MEETING__CANCEL_MEETING_REQUEST_IS_NULL");
            }

            string meetingId = cancelRequest.ReferanceId;
            if (string.IsNullOrWhiteSpace(meetingId))
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.CancelMeeting | Cancel request referance id is null or empty.");
                throw new UDArgumentException("PERMIT_MEETING_CANCEL_MEETING_REQUEST_REFERANCE_ID_IS_NULL_OR_EMPTY");
            }

            await _graphClient.Users[_organizerEmail].Events[meetingId].Cancel.PostAsync(new CancelPostRequestBody
            {
                Comment = cancelRequest.Comment                
            });

            return new BaseResponse<bool> { Data = true, Message = "PERMIT_MEETING_CANCEL_MEETING_REQUEST_SUCCESS", Success = true };
        }

        #endregion  Public Methods

        #region Private Methods

        private string ValidateForwardRequestDto(ForwardMeetingRequest forwardRequest)
        {
            if (forwardRequest == null)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateForwardRequestDto | Forward request dto is null.");
                return "PERMIT_MEETING_FORWARD_REQUEST_IS_NULL";
            }

            if (string.IsNullOrWhiteSpace(forwardRequest.ReferanceId))
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateForwardRequestDto | Forward meeting request referance id is null or empty.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_REFERANCE_ID_IS_NULL_OR_EMPTY";
            }

            List<EmailAddressDto> toRecipients = forwardRequest.ToRecipients;
            if (toRecipients == null || !toRecipients.Any())
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateForwardRequestDto | Forward request ToRecipients list is null or empty.");
                return "PERMIT_MEETING_FORWARD_REQUEST_TO_RECIPIENTS_NULL_OR_EMPTY";
            }

            return string.Empty;
        }

        private IList<AvailabilityDto>? GetAvailabilityDtoListByGraphSchedulePostResponse(GetSchedulePostResponse? schedulePostResponse)
        {
            if (schedulePostResponse == null)
            {
                _logger.LogWarning("DDOT.MPS.Communication.Api.Managers.MeetingManager.GetAvailabilityDtoListByGraphSchedulePostResponse | schedulePostResponse entity is null.");
                return null;
            }

            List<ScheduleInformation>? scheduleInformationList = schedulePostResponse.Value;
            if (scheduleInformationList == null || !scheduleInformationList.Any())
            {
                _logger.LogWarning("DDOT.MPS.Communication.Api.Managers.MeetingManager.GetAvailabilityDtoListByGraphSchedulePostResponse | scheduleInformationList is null or empty.");
                return null;
            }

            IList<AvailabilityDto> availabilityDtoList = new List<AvailabilityDto>();
            AvailabilityDto availabilityDto = null;
            foreach (ScheduleInformation scheduleInformation in scheduleInformationList)
            {
                availabilityDto = new AvailabilityDto { ScheduleId = scheduleInformation.ScheduleId };
                availabilityDtoList.Add(availabilityDto);

                FreeBusyError? freeBusyError = scheduleInformation.Error;
                if (freeBusyError != null)
                {
                    availabilityDto.ErrorMessage = freeBusyError.Message;
                    continue;
                }

                List<ScheduleItem>? scheduleItems = scheduleInformation.ScheduleItems;
                availabilityDto.Available = scheduleItems == null || !scheduleItems.Any();

                if (!availabilityDto.Available.Value)
                {
                    List<AvailabileMeeting> meetings = new List<AvailabileMeeting>();
                    foreach (ScheduleItem scheduleItem in scheduleItems!)
                    {
                        meetings.Add(new AvailabileMeeting
                        {
                            Start = scheduleItem.Start?.DateTime,
                            End = scheduleItem.End?.DateTime,
                            Status = GetFreeBusyStatusDisplayString(scheduleItem.Status)
                        });
                    }


                    availabilityDto.Meetings = meetings;
                }
            }

            return availabilityDtoList;
        }

        private string ValidateAvailabilityRequestDto(AvailabilityRequest availabilityRequest)
        {
            if (availabilityRequest == null)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateAvailabilityRequestDto | Availability request dto is null.");
                return "PERMIT_MEETING_AVAILABILITY_REQUEST_IS_NULL";
            }

            List<string> schedules = availabilityRequest.Schedules;
            if (schedules == null || !schedules.Any())
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateAvailabilityRequestDto | Availability request schedules list is null or empty.");
                return "PERMIT_MEETING_AVAILABILITY_REQUEST_SCHEDULES_NULL_OR_EMPTY";
            }

            DateTime scheduleStartDateTime = availabilityRequest.Start;
            DateTime scheduleEndDateTime = availabilityRequest.End;
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById(MeetingConstants.EasternStandardTimeZone);
            DateTime nowEST = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);

            if (scheduleStartDateTime <= nowEST)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateAvailabilityRequestDto | Availability request start date time is invalid.");
                return "PERMIT_MEETING_AVAILABILITY_REQUEST_STARTDATE_IS_INVALID";
            }

            if (scheduleEndDateTime <= nowEST)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateAvailabilityRequestDto | Availability request end date time is invalid.");
                return "PERMIT_MEETING_AVAILABILITY_REQUEST_ENDDATE_IS_INVALID";
            }

            if (scheduleEndDateTime <= scheduleStartDateTime)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateAvailabilityRequestDto | Availability request end date time is less than or equal to start date time.");
                return "PERMIT_MEETING_AVAILABILITY_REQUEST_INVALID_DATE_RANGE";
            }

            int differenceInMinutes = (int)(scheduleEndDateTime - scheduleStartDateTime).TotalMinutes;
            if (differenceInMinutes > 1440 || differenceInMinutes < 5)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateAvailabilityRequestDto | Availability request schedule time differance is invalid. | Maximum should be 1440 miniutes and minimum should be 5 miniutes. | Difference in minutes {differenceInMinutes}", differenceInMinutes);
                return "PERMIT_MEETING_AVAILABILITY_REQUEST_DATE_RANGE_DIFFERENCE_INVALID";
            }

            return string.Empty;
        }

        private static string GetFreeBusyStatusDisplayString(FreeBusyStatus? freeBusyStatus)
        {
            return freeBusyStatus switch
            {
                null => "",
                FreeBusyStatus.Unknown => "Unknown",
                FreeBusyStatus.Free => "Free",
                FreeBusyStatus.Tentative => "Tentative",
                FreeBusyStatus.Busy => "Busy",
                FreeBusyStatus.Oof => "Oof",
                FreeBusyStatus.WorkingElsewhere => "Working Elsewhere",
                _ => freeBusyStatus.ToString() ?? ""
            };
        }

        private MeetingDto? GetMeetingDtoByGraphEvent(Event? meeting)
        {
            if (meeting == null)
            {
                _logger.LogWarning("DDOT.MPS.Communication.Api.Managers.MeetingManager.GetMeetingDtoByGraphEvent | Meeting entity is null.");
                return null;
            }

            return new MeetingDto
            {
                ReferanceId = meeting.Id,
                CreatedDateTime = meeting.CreatedDateTime?.UtcDateTime,
                LastModifiedDateTime = meeting.LastModifiedDateTime?.UtcDateTime,
                Start = meeting.Start?.DateTime,
                End = meeting.End?.DateTime,
                Subject = meeting.Subject,
                WebLink = meeting.WebLink ?? string.Empty,
                OnlineMeetingUrl = meeting.OnlineMeeting?.JoinUrl ?? string.Empty,
                Organizer = GetEmailAddressDto(meeting.Organizer?.EmailAddress),
                Attendees = GetAttendeeDtoList(meeting.Attendees)
            };
        }

        private static EmailAddressDto GetEmailAddressDto(EmailAddress? meetingOrganizerEmailAddress)
        {
            EmailAddressDto emailAddress = new EmailAddressDto();
            if (meetingOrganizerEmailAddress != null)
            {
                emailAddress.Address = meetingOrganizerEmailAddress.Address;
                emailAddress.Name = meetingOrganizerEmailAddress.Name;
            }

            return emailAddress;
        }

        private static List<AttendeeDto> GetAttendeeDtoList(List<Attendee>? meetingAttendees)
        {
            List<AttendeeDto> attendees = new List<AttendeeDto>();
            if (meetingAttendees != null && meetingAttendees.Any())
            {
                foreach (Attendee meetingAttendee in meetingAttendees)
                {
                    EmailAddress? meetingAttendeeEmailAddress = meetingAttendee.EmailAddress;
                    ResponseStatus? meetingAttendeeStatus = meetingAttendee.Status;
                    attendees.Add(new AttendeeDto
                    {
                        EmailAddress = new EmailAddressDto
                        {
                            Address = meetingAttendeeEmailAddress?.Address,
                            Name = meetingAttendeeEmailAddress?.Name
                        },
                        Response = GetResponseTypeDisplayString(meetingAttendeeStatus?.Response),
                        ResponseTime = meetingAttendeeStatus?.Time?.UtcDateTime,
                        Type = GetAttendeeTypeeDisplayString(meetingAttendee.Type)
                    });
                }
            }

            return attendees;
        }

        private static string GetAttendeeTypeeDisplayString(AttendeeType? attendanceType)
        {
            return attendanceType switch
            {
                null => "",
                AttendeeType.Required => "Required",
                AttendeeType.Optional => "Optional",
                AttendeeType.Resource => "Resource",
                _ => attendanceType.ToString() ?? ""
            };
        }

        private static string GetResponseTypeDisplayString(ResponseType? responseType)
        {
            return responseType switch
            {
                null => "",
                ResponseType.None => "None",
                ResponseType.Organizer => "Organizer",
                ResponseType.TentativelyAccepted => "Tentatively Accepted",
                ResponseType.Accepted => "Accepted",
                ResponseType.Declined => "Declined",
                ResponseType.NotResponded => "Not Responded",
                _ => responseType.ToString() ?? ""
            };
        }

        private Event GetEventByMeetingRequestDto(CreateMeetingRequest createMeetingRequest, string easternStandardTimeZone)
        {
            string? locationName = createMeetingRequest.Location;
            return new Event
            {
                Subject = createMeetingRequest.Subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = createMeetingRequest.Body,
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = createMeetingRequest.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = easternStandardTimeZone,
                },
                End = new DateTimeTimeZone
                {
                    DateTime = createMeetingRequest.End.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = easternStandardTimeZone,
                },
                AllowNewTimeProposals = true,
                IsOnlineMeeting = true,
                OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness,
                Attendees = createMeetingRequest.Attendees.Select(a => new Attendee
                {
                    Type = Enum.TryParse(a.Type, out AttendeeType attendeeType) ? attendeeType : AttendeeType.Required,
                    EmailAddress = new EmailAddress
                    {
                        Address = a.EmailAddress.Address,
                        Name = a.EmailAddress.Name,
                    }
                }).ToList(),
                Location = string.IsNullOrWhiteSpace(locationName) ? null : new Location { DisplayName = locationName }
            };
        }

        private string ValidateMeetingRequestDto(CreateMeetingRequest createMeetingRequest, string easternStandardTimeZone)
        {
            if (createMeetingRequest == null)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request dto is null.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_IS_NULL";
            }

            if (string.IsNullOrWhiteSpace(createMeetingRequest.Subject))
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request subject is null or empty.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_SUBJECT_IS_NULL_OR_EMPTY";
            }

            if (string.IsNullOrWhiteSpace(createMeetingRequest.Body))
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request body is null or empty.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_BODY_IS_NULL_OR_EMPTY";
            }

            DateTime meetingStartDateTime = createMeetingRequest.Start;
            DateTime meetingEndDateTime = createMeetingRequest.End;
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById(easternStandardTimeZone);
            DateTime nowEST = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);

            if (meetingStartDateTime <= nowEST)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request start date time is invalid.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_STARTDATE_IS_INVALID";
            }

            if (meetingEndDateTime <= nowEST)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request end date time is invalid.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_ENDDATE_IS_INVALID";
            }

            if (meetingEndDateTime <= meetingStartDateTime)
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request end date time is less than or equal to start date time.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_INVALID_DATE_RANGE";
            }

            _logger.LogInformation("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Converting attendees to dictionary.");
            IList<AttendeeDtoBase> attendees = createMeetingRequest.Attendees;
            if (attendees == null || !attendees.Any())
            {
                _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request attendees list is null or empty.");
                return "PERMIT_MEETING_CREATE_MEETING_REQUEST_ATTENDEES_NULL_OR_EMPTY";
            }

            foreach (AttendeeDtoBase attendee in attendees)
            {
                EmailAddressDto emailAddress = attendee.EmailAddress;
                if (emailAddress == null || string.IsNullOrWhiteSpace(emailAddress.Address))
                {
                    _logger.LogError("DDOT.MPS.Communication.Api.Managers.MeetingManager.ValidateCreateMeetingRequestDto | Create meeting request attendees, selected attendee is null or invalid. | Attendees: {attendees}", JsonSerializer.Serialize(attendees));
                    return "PERMIT_MEETING_CREATE_MEETING_REQUEST_ATTENDEES_INVALID";
                }
            }

            return string.Empty;
        }
           
        #endregion  Private Methods
    }
}
