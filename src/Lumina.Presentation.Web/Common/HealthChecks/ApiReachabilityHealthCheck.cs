#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.HealthChecks;

/// <summary>
/// Health check that verifies the remote API is reachable.
/// </summary>
public class ApiReachabilityHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ServerConfigurationDto _serverConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiReachabilityHealthCheck"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Injected factory for creating HTTP clients.</param>
    /// <param name="serverConfigurationOptions">Injected server configuration application settings.</param>
    public ApiReachabilityHealthCheck(IHttpClientFactory httpClientFactory, IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions)
    {
        _httpClient = httpClientFactory.CreateClient();
        _serverConfiguration = serverConfigurationOptions.Value;
    }

    /// <summary>
    /// Checks whether the remote API is reachable by probing its readiness endpoint.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the health check result.</returns>
    public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        string healthUrl = $"{_serverConfiguration.BaseAddress}:{_serverConfiguration.Port}/health/ready";
        using CancellationTokenSource timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(healthUrl, timeoutCancellationTokenSource.Token).ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"The API returned status {(int)response.StatusCode}.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("The API is not reachable.", exception);
        }
    }
}
