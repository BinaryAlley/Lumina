#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Common.Setup;

/// <summary>
/// API reachability health check that always reports a healthy state.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class HealthyApiReachabilityHealthCheck : ApiReachabilityHealthCheck
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthyApiReachabilityHealthCheck"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Injected factory for creating HTTP clients.</param>
    /// <param name="serverConfigurationOptions">Injected server configuration application settings.</param>
    public HealthyApiReachabilityHealthCheck(IHttpClientFactory httpClientFactory, IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions)
        : base(httpClientFactory, serverConfigurationOptions)
    {
    }

    /// <summary>
    /// Reports a healthy state without probing the API.
    /// </summary>
    public override Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
