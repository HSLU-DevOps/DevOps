using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebAPI.Persistence;

namespace WebAPI.Controllers;

public class HealthCheckDb(TodoDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                return new HealthCheckResult(HealthStatus.Healthy, "Database is connected");
            }
            return new HealthCheckResult(HealthStatus.Unhealthy, "Database is not connected");
        }
        catch (Exception e)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, "Database is not connected", e);
        }
    }
}