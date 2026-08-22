#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.HealthChecks;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Health;

/// <summary>
/// Factory whose database health check always reports an unhealthy state.
/// </summary>
[ExcludeFromCodeCoverage]
public class FailingDatabaseApiFactory : LuminaApiFactory
{
    /// <summary>
    /// Configures the web host, replacing the database health check with one that always fails.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            // the health check resolves DatabaseHealthCheck from DI when it runs, so replacing its registration with a failing subclass makes /health/ready report Unhealthy
            services.RemoveAll<DatabaseHealthCheck>();
            services.AddScoped<DatabaseHealthCheck, FailingDatabaseHealthCheck>();
        });
    }
}
