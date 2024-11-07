#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Factory for creating a web application for integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class LuminaApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaApiFactory"/> class.
    /// </summary>
    public LuminaApiFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    /// <summary>
    /// Configures the web host for the integration tests.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // clear all existing configuration sources
            config.Sources.Clear();

            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.test.json", optional: true, reloadOnChange: true);
            config.AddJsonFile("appsettings.shared.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.shared.test.json", optional: true, reloadOnChange: true);
            // First add the test values directly
            config.AddInMemoryCollection(initialData: new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-key-thats-at-least-32-chars-long-for-jwt",
                ["EncryptionSettings:SecretKey"] = "dGVzdC1rZXktdGhhdHMtYXQtbGVhc3QtMzItY2hhcnMtbG9uZy1mb3ItZW5jcnlwdGlvbg==" // base64 encoded test key
            });
        });
        builder.ConfigureServices(services =>
        {
            // remove existing DbContext configuration
            ServiceDescriptor? descriptor = services.SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);
            // add SQLite DbContext configuration
            services.AddDbContext<LuminaDbContext>(options => options.UseSqlite(_connection));
            // build service provider and ensure database is created
            ServiceProvider servicePreovider = services.BuildServiceProvider();
            using (IServiceScope scope = servicePreovider.CreateScope())
            {
                IServiceProvider scopedServices = scope.ServiceProvider;
                LuminaDbContext dbContext = scopedServices.GetRequiredService<LuminaDbContext>();
                dbContext.Database.Migrate(); 
            }
        });
    }

    /// <summary>
    /// Disposes the connection to the database.
    /// </summary>
    public new void Dispose()
    {
        _connection.Close();
        base.Dispose();
    }
}
