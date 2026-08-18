#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Seed;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

// <summary>
/// Factory that extends <see cref="LuminaApiFactory"/> to provide authenticated HTTP clients for testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthenticatedLuminaApiFactory : LuminaApiFactory, IDisposable
{
    private readonly PasswordHashService _hashService = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Gets the username of the currently created test user.
    /// </summary>
    public string? TestUsername { get; private set; }

    /// <summary>
    /// Creates an HTTP client with authentication headers and a test user
    /// </summary>
    /// <returns>An HTTP client configured with auth headers</returns>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = CreateClient();
        TestUsername = $"testuser_{Guid.NewGuid()}";

        // add the X-Forwarded-For header
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}");

        // perform authentication 
        await CreateAndAuthenticateUser(client);

        return client;
    }

    /// <summary>
    /// Creates and authenticates a test user.
    /// </summary>
    /// <param name="client">The HTTP client to configure with auth.</param>
    /// <returns>The created user entity.</returns>
    private async Task<UserEntity> CreateAndAuthenticateUser(HttpClient client)
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // create test user
        UserEntity user = new()
        {
            Id = Guid.NewGuid(),
            Username = TestUsername!,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // authenticate user
        LoginRequest loginRequest = new(
            Username: user.Username,
            Password: "TestPass123!"
        );

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        // set auth header for subsequent requests
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

        return user;
    }

    /// <summary>
    /// Creates an HTTP client with authentication headers and an admin test user with all permissions
    /// </summary>
    /// <returns>An HTTP client configured with admin auth headers</returns>
    public async Task<HttpClient> CreateAuthenticatedAdminClientAsync()
    {
        HttpClient client = CreateClient();
        TestUsername = $"testuser_{Guid.NewGuid()}";

        // add the X-Forwarded-For header
        client.DefaultRequestHeaders.Add("X-Forwarded-For",
            $"192.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}");

        // create and authenticate admin user
        await CreateAndAuthenticateAdminUser(client);

        return client;
    }

    /// <summary>
    /// Creates and authenticates an admin test user with all permissions.
    /// </summary>
    /// <param name="client">The HTTP client to configure with auth.</param>
    /// <returns>The created user entity.</returns>
    private async Task<UserEntity> CreateAndAuthenticateAdminUser(HttpClient client)
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        IDataSeedService dataSeedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();

        // create test user
        UserEntity user = new()
        {
            Id = Guid.NewGuid(),
            Username = TestUsername!,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // set up admin permissions and roles
        await dataSeedService.SetDefaultAuthorizationPermissionsAsync(user.Id, CancellationToken.None);
        await dataSeedService.SetDefaultAuthorizationRolesAsync(user.Id, CancellationToken.None);
        await dataSeedService.SetAdminRolePermissionsAsync(user.Id, CancellationToken.None);
        await dataSeedService.SetAdminRoleToAdministratorAccount(user.Id, CancellationToken.None);

        // authenticate user
        LoginRequest loginRequest = new(
            Username: user.Username,
            Password: "TestPass123!"
        );

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        // set auth header for subsequent requests
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

        return user;
    }

    /// <summary>
    /// Removes the currently created test user from the database.
    /// </summary>
    /// <remarks>
    /// Test classes must call this in their per-test <c>DisposeAsync</c> instead of <see cref="Dispose"/>. The
    /// factory is a shared <see cref="IClassFixture{TFixture}"/>, created once per test class and disposed by
    /// xUnit only after all tests in the class have run. Calling <see cref="Dispose"/> from a per-test
    /// <c>DisposeAsync</c> would dispose the web host and its service provider after the first test, making
    /// every subsequent test in the class fail with an <see cref="ObjectDisposedException"/> when it resolves
    /// services. <see cref="Dispose"/> is therefore reserved for xUnit's own class-teardown and must never be
    /// invoked from a test.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RemoveTestUserAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == TestUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    /// <remarks>
    /// Called by xUnit when the test class completes. Do not call from tests; per-test cleanup must use
    /// <see cref="RemoveTestUserAsync"/>. See that method for the details.
    /// </remarks>
    public new void Dispose()
    {
        using IServiceScope scope = Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == TestUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
        }

        base.Dispose();
    }
}
