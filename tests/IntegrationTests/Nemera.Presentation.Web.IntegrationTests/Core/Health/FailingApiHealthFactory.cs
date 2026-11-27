#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.HealthChecks;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Health;

/// <summary>
/// Factory whose API reachability health check always reports an unhealthy state.
/// </summary>
[ExcludeFromCodeCoverage]
public class FailingApiHealthFactory : LuminaWebFactory
{
    /// <summary>
    /// Configures the web host, replacing the API reachability health check with one that always fails.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApiReachabilityHealthCheck>();
            services.AddScoped<ApiReachabilityHealthCheck, UnhealthyApiReachabilityHealthCheck>();
        });
    }
}
