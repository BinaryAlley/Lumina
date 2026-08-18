#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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
    /// Gets a unique IP address derived from a fresh GUID, to isolate the rate-limiting partition of each test.
    /// </summary>
    /// <returns>A unique IP address in the 192.168.0.0/16 private range.</returns>
    public static string GetUniqueTestIp()
    {
        byte[] bytes = Guid.NewGuid().ToByteArray();
        return $"192.168.{bytes[0]}.{bytes[1]}";
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
        client.DefaultRequestHeaders.Add("X-Forwarded-For", GetUniqueTestIp());

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
    /// Creates a test user, authenticates it on <paramref name="client"/>, and returns its Id and username.
    /// </summary>
    /// <param name="client">The HTTP client to configure with authentication headers.</param>
    /// <returns>The Id and username of the created test user.</returns>
    public async Task<(Guid UserId, string Username)> CreateAndAuthenticateUserAsync(HttpClient client)
    {
        // a unique X-Forwarded-For isolates rate limiting state per test
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", GetUniqueTestIp());

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

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(username, "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        return (userId, username);
    }

    /// <summary>
    /// Removes a test user and its owned libraries from the database.
    /// </summary>
    /// <param name="username">The username of the test user to remove.</param>
    public async Task RemoveTestUserAsync(string username)
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = await dbContext.Users.Include(candidate => candidate.Libraries).FirstOrDefaultAsync(candidate => candidate.Username == username).ConfigureAwait(false);
        if (user is not null)
        {
            dbContext.Libraries.RemoveRange(user.Libraries);
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes a test admin user, its role, and its user-role link from the database.
    /// </summary>
    /// <param name="userId">The Id of the admin test user to remove.</param>
    public async Task RemoveAdminUserAsync(Guid userId)
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserRoleEntity? userRole = await dbContext.UserRoles.FirstOrDefaultAsync(candidate => candidate.UserId == userId).ConfigureAwait(false);
        if (userRole is not null)
        {
            RoleEntity? role = await dbContext.Roles.FirstOrDefaultAsync(candidate => candidate.Id == userRole.RoleId).ConfigureAwait(false);
            if (role is not null)
                dbContext.Roles.Remove(role);
            dbContext.UserRoles.Remove(userRole);
        }
        UserEntity? user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId).ConfigureAwait(false);
        if (user is not null)
            dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    public async Task SeedLibraryAsync(Guid libraryId, Guid userId)
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(new LibraryEntity
        {
            Id = libraryId,
            UserId = userId,
            Title = "Test Library",
            LibraryType = LibraryType.EBook,
            ContentLocations = [],
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a <see cref="BookEntity"/> belonging to the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="title">The title of the book.</param>
    public async Task SeedBookAsync(Guid libraryId, string title)
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Books.Add(new BookEntity
        {
            Id = Guid.NewGuid(),
            LibraryId = libraryId,
            Path = $"/books/{Guid.NewGuid()}.epub",
            Title = title,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
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
