#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.HealthChecks;
using Lumina.Presentation.Web.Fixtures.Common.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Common.Setup;

/// <summary>
/// Factory for creating the Web application for integration tests, with the remote API client replaced by a configurable stub.
/// </summary>
[ExcludeFromCodeCoverage]
public class LuminaWebFactory : WebApplicationFactory<Program>, IDisposable
{
    private const string TEST_ENCRYPTION_KEY = "FLYO0QRo6u2VzoFOgNkkEwYNGtqhJ3QGZd7iAHNEJeM=";

    /// <summary>
    /// Gets the stub used in place of the remote API client.
    /// </summary>
    public StubApiHttpClient ApiClientStub { get; } = new();

    /// <summary>
    /// Gets the stub used in place of the remote streaming API client.
    /// </summary>
    public StubStreamingApiHttpClient StreamingApiClientStub { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaWebFactory"/> class.
    /// </summary>
    public LuminaWebFactory()
    {
    }

    /// <summary>
    /// Configures the web host for the integration tests.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Provide the application configuration values that the Web app validates on startup.
            config.AddInMemoryCollection(initialData: new Dictionary<string, string?>
            {
                ["EncryptionSettings:SecretKey"] = TEST_ENCRYPTION_KEY,
                ["ServerConfiguration:ApiVersion"] = "1",
                ["ServerConfiguration:BaseAddress"] = "http://localhost",
                ["ServerConfiguration:Port"] = "5214"
            });
        });
        builder.ConfigureServices(services =>
        {
            // Remove the typed HTTP client registration, so that the stub can take its place.
            ServiceDescriptor[] apiClientDescriptors = [.. services.Where(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IApiHttpClient))];
            foreach (ServiceDescriptor descriptor in apiClientDescriptors)
                services.Remove(descriptor);
            services.AddSingleton<IApiHttpClient>(ApiClientStub);

            // Remove the typed streaming HTTP client registration, so that the streaming stub can take its place.
            ServiceDescriptor[] streamingApiClientDescriptors = [.. services.Where(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IStreamingApiHttpClient))];
            foreach (ServiceDescriptor descriptor in streamingApiClientDescriptors)
                services.Remove(descriptor);
            services.AddSingleton<IStreamingApiHttpClient>(StreamingApiClientStub);

            // The API reachability health check resolves ApiReachabilityHealthCheck from DI when it runs; replace its registration with a healthy subclass, since no real API exists during these tests.
            services.RemoveAll<ApiReachabilityHealthCheck>();
            services.AddScoped<ApiReachabilityHealthCheck, HealthyApiReachabilityHealthCheck>();
        });
    }
}
