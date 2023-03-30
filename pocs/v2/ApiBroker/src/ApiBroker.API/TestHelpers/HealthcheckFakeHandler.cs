using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiBroker.API.TestHelpers;

public class HealthcheckFakeHandler : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var sla = RandomDataUtils.SomeInt(100);
        return sla > 90
            ? Task.FromResult(HealthCheckResult.Unhealthy("Unhealthy"))
            : Task.FromResult(sla > 80
                ? HealthCheckResult.Degraded("Degraded")
                : HealthCheckResult.Healthy("Healthy"));
    }
}