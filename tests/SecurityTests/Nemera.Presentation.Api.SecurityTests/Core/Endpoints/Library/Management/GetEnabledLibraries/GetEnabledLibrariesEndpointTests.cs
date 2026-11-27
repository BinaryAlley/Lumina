#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.Contracts.Fixtures.Core.Requests.Authentication;
using Nemera.Contracts.Responses.Authentication;
using Nemera.DataAccess.Core.UoW;
using Nemera.Infrastructure.Core.Security;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// Contains security tests for the <see cref="GetEnabledLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesEndpointTests : IClassFixture<NemeraApiFactory>
{
    private readonly HttpClient _client;
    private readonly NemeraApiFactory _apiFactory;
    private readonly PasswordHashService _hashService = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LoginRequestFixture _loginRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetEnabledLibrariesEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetEnabledLibraries_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/enabled");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/libraries/enabled", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task GetEnabledLibraries_WhenAuthenticated_ShouldNotLeakSensitiveData()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await CreateAndAuthenticateUserAsync(client);

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/libraries/enabled");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CreateAndAuthenticateUserAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.{Random.Shared.Next(1, 255)}");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"testuser_{Guid.NewGuid()}";
        UserEntity user = _userEntityFixture.Create(id: userId, username: username, password: _hashService.HashString("TestPass123!"));
        user.TotpSecret = null;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", _loginRequestFixture.Create(username: username, password: "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
    }
}
