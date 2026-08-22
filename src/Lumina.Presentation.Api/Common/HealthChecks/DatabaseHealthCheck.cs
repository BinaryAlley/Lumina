#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Common.HealthChecks;

/// <summary>
/// Health check that verifies the database can be reached.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly LuminaDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="dbContext">Injected database context.</param>
    public DatabaseHealthCheck(LuminaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Checks whether the database can be reached.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the health check result.</returns>
    public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The database is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("The database is not reachable.", exception);
        }
    }
}
