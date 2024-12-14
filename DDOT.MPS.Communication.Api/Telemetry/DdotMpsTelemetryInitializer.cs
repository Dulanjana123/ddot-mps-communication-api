using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace DDOT.MPS.Communication.Api.Telemetry
{
    public class DdotMpsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DdotMpsTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is TraceTelemetry traceTelemetry) AddCustomProperties(traceTelemetry.Properties);
            else if (telemetry is RequestTelemetry requestTelemetry) AddCustomProperties(requestTelemetry.Properties);
            else if (telemetry is RequestTelemetry exceptionTelemetry) AddCustomProperties(exceptionTelemetry.Properties);
        }

        private void AddCustomProperties(IDictionary<string, string> customProperties)
        {
            customProperties["App"] = "DDOT.MPS.Communication";

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                if (httpContext.Request.Headers.TryGetValue("x-correlation-id", out var correlationId)) customProperties["CorrelationId"] = correlationId.ToString();
                //customProperties["UserId"] = httpContext.User?.FindFirst("user-id")?.Value ?? "NA";
            }
        }
    }
}
