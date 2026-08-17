#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Fixtures.Common.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            // provide the application configuration values that the Web app validates on startup
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
            // remove the typed HTTP client registration, so that the stub can take its place
            ServiceDescriptor[] apiClientDescriptors = services.Where(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IApiHttpClient)).ToArray();
            foreach (ServiceDescriptor descriptor in apiClientDescriptors)
                services.Remove(descriptor);
            services.AddSingleton<IApiHttpClient>(ApiClientStub);
        });
    }
}
