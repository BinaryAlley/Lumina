#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Common.Setup;

/// <summary>
/// Factory for creating a web application for security tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class LuminaApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PasswordHashService _hashService = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private const string TEST_ENCRYPTION_KEY = "MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaApiFactory"/> class.
    /// </summary>
    public LuminaApiFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    /// <summary>
    /// Configures the web host for the security tests.
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
                ["JwtSettings:ExpiryMinutes"] = "15", // control the expiry time during tests
                ["JwtSettings:Issuer"] = "Lumina",
                ["JwtSettings:Audience"] = "Lumina",
                ["EncryptionSettings:SecretKey"] = TEST_ENCRYPTION_KEY // base64 encoded test key
            });
        });
        builder.ConfigureServices(services =>
        {
            // remove existing DbContext configuration
            ServiceDescriptor? descriptor = services.SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);
            // add SQLite DbContext configuration
            services.AddDbContext<LuminaDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(serviceProvider.GetRequiredService<UpdateAuditableEntitiesInterceptor>());
            });
            // configure JWT authentication for testing
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "Lumina",
                    ValidAudience = "Lumina",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("test-key-thats-at-least-32-chars-long-for-jwt"))
                };
            });
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
    /// Creates a test user with the Admin role, authenticates it on <paramref name="client"/>, and returns its Id.
    /// </summary>
    /// <param name="client">The HTTP client to configure with authentication headers.</param>
    /// <returns>The Id of the created admin test user.</returns>
    public async Task<Guid> CreateAndAuthenticateAdminUserAsync(HttpClient client)
    {
        // a unique X-Forwarded-For isolates rate limiting state per test
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.{Random.Shared.Next(1, 255)}");

        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        Guid userId = Guid.NewGuid();
        string username = $"testuser_{Guid.NewGuid()}";
        UserEntity user = new()
        {
            Id = userId,
            Username = username,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // seed a dedicated Admin role for this test user, keeping each test isolated in the shared in-memory database
        Guid roleId = Guid.NewGuid();
        RoleEntity role = new()
        {
            Id = roleId,
            RoleName = "Admin",
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow
        };
        dbContext.Roles.Add(role);
        dbContext.UserRoles.Add(new UserRoleEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            RoleId = roleId,
            Role = role,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(username, "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        return userId;
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
