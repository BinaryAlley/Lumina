#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
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
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly SqliteConnection _connection;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaApiFactory"/> class.
    /// </summary>
    public LuminaApiFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Configures the web host for the integration tests.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);
            services.AddDbContext<LuminaDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
            var servicePreovider = services.BuildServiceProvider();
            using (var scope = servicePreovider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<LuminaDbContext>();
                dbContext.Database.EnsureCreated();
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
    #endregion
}