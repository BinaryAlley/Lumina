#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

// <summary>
/// Factory that extends <see cref="LuminaApiFactory"/> to provide authenticated HTTP clients for testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthenticatedLuminaApiFactory : LuminaApiFactory, IDisposable
{
    private readonly HashService _hashService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Gets the username of the currently created test user.
    /// </summary>
    public string? TestUsername { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatedLuminaApiFactory"/> class.
    /// </summary>
    public AuthenticatedLuminaApiFactory()
    {
        _hashService = new HashService();
    }

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
            UserRoles = [],
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
    /// Disposes API factory resources.
    /// </summary>
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
    }
}
