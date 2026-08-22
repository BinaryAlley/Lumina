#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Common.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Health;

/// <summary>
/// Database health check that always reports an unhealthy state.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class FailingDatabaseHealthCheck : DatabaseHealthCheck
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailingDatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="dbContext">Injected database context.</param>
    public FailingDatabaseHealthCheck(LuminaDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Reports an unhealthy state without probing the database.
    /// </summary>
    public override Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckResult.Unhealthy("database unavailable"));
    }
}
