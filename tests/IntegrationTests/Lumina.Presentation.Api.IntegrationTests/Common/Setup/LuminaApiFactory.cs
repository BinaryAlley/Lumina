#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.UoW;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Factory for creating a web application for integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class LuminaApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _sharedInMemoryConnectionString;
    private const string TEST_ENCRYPTION_KEY = "MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=";
    private const string SHARED_IN_MEMORY_CONNECTION_STRING_PREFIX = "Data Source=lumina-tests-{0};Mode=Memory;Cache=Shared";
    private static byte s_IpClass = 25;

    public static byte IpClass => s_IpClass++;

    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaApiFactory"/> class.
    /// </summary>
    public LuminaApiFactory()
    {
        // Each factory gets its own uniquely named shared in-memory database, so that parallel test classes remain isolated from one another;
        // the shared cache allows each DbContext to open its own pooled connection to the same database, instead of sharing a single connection,
        // which would otherwise serialize every request on one connection and cause nested transaction errors between requests.
        _sharedInMemoryConnectionString = string.Format(SHARED_IN_MEMORY_CONNECTION_STRING_PREFIX, Guid.NewGuid());
        // Keep a single anchor connection open, so that the shared in-memory database it refers to is not dropped.
        _connection = new SqliteConnection(_sharedInMemoryConnectionString);
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
            // Clear all existing configuration sources.
            config.Sources.Clear();

            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.test.json", optional: true, reloadOnChange: true);
            config.AddJsonFile("appsettings.shared.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.shared.test.json", optional: true, reloadOnChange: true);
            // First add the test values directly.
            config.AddInMemoryCollection(initialData: new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-key-thats-at-least-32-chars-long-for-jwt",
                ["JwtSettings:ExpiryMinutes"] = "60",
                ["JwtSettings:Issuer"] = "test-issuer",
                ["JwtSettings:Audience"] = "test-audience",
                ["CorsSettings:AllowedOrigins:0"] = "http://localhost:5012",
                ["EncryptionSettings:SecretKey"] = TEST_ENCRYPTION_KEY // Base64 encoded test key.
            });
        });
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext configuration.
            ServiceDescriptor? descriptor = services.SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);
            // Add SQLite DbContext configuration.
            services.AddDbContext<LuminaDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite(_sharedInMemoryConnectionString);
                options.AddInterceptors(serviceProvider.GetRequiredService<UpdateAuditableEntitiesInterceptor>());
                ConfigureDbContextOptions(serviceProvider, options);
            });
            // Configure JWT authentication for testing.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "test-issuer",
                    ValidAudience = "test-audience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("test-key-thats-at-least-32-chars-long-for-jwt"))
                };
            });
            // Build service provider and ensure database is created.
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
    /// Configures additional options of the database context used by the tests.
    /// </summary>
    /// <remarks>
    /// The hook is invoked every time the options of a database context are built, including once for the throwaway provider that migrates the
    /// database while the host is configured and once for the final provider of the host. Derived factories that resolve test services from the
    /// provided <paramref name="serviceProvider"/> must register those services before the shared factory configures the database, and must call
    /// this base implementation so that options added here are never silently skipped.
    /// </remarks>
    /// <param name="serviceProvider">The service provider used to resolve the options of the database context.</param>
    /// <param name="options">The options builder of the database context being configured.</param>
    protected virtual void ConfigureDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
    }

    /// <summary>
    /// Resets the in-memory SQLite database to its original state.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext context = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        context.Books.RemoveRange(context.Books);
        context.Libraries.RemoveRange(context.Libraries);
        context.RolePermissions.RemoveRange(context.RolePermissions);
        context.UserRoles.RemoveRange(context.UserRoles);
        context.Roles.RemoveRange(context.Roles);
        context.Permissions.RemoveRange(context.Permissions);
        context.Users.RemoveRange(context.Users);
        context.UserPermissions.RemoveRange(context.UserPermissions);

        await context.SaveChangesAsync();
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
