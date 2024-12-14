using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DDOT.MPS.Communication.Api
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly string _environmentName;

        public CustomHealthCheck(string environmentName)
        {
            _environmentName = environmentName;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string message = $"API is Up and Running in {_environmentName} environment";
            return Task.FromResult(HealthCheckResult.Healthy(message));
        }
    }
}
