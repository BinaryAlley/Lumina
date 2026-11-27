#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.Contracts.Fixtures.Core.Requests.Authentication;
using Nemera.Contracts.Requests.Authentication;
using Nemera.Contracts.Responses.Authentication;
using Nemera.DataAccess.Core.UoW;
using Nemera.Infrastructure.Core.Security;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains security tests for the <see cref="ChangePasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointTests : IClassFixture<NemeraApiFactory>, IDisposable
{
    private readonly NemeraApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly PasswordHashService _hashService = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly ChangePasswordRequestFixture _changePasswordRequestFixture = new();
    private readonly LoginRequestFixture _loginRequestFixture = new();
    private readonly string _testUsername;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ChangePasswordEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.12");
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(
            username: _testUsername,
            currentPassword: "OldPass123!",
            newPassword: "NewPass123!",
            newPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/auth/change-password", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ShouldReturnUniformForbiddenResponse()
    {
        // Arrange
        UserEntity user = await CreateAndAuthenticateUser();
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(
            username: user.Username,
            currentPassword: "WrongPass123!",
            newPassword: "NewPass123!",
            newPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage firstResponse = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);
        string firstContent = await firstResponse.Content.ReadAsStringAsync();
        HttpResponseMessage secondResponse = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);
        string secondContent = await secondResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, secondResponse.StatusCode);
        AssertProblemDetail(firstContent, "General.Failure", "InvalidCurrentPassword");
        AssertProblemDetail(secondContent, "General.Failure", "InvalidCurrentPassword");
    }

    [Theory]
    [InlineData("'; DROP TABLE Users--")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    public async Task ChangePassword_WithSQLInjectionInUsername_ShouldNotCorruptOrDeleteData(string maliciousUsername)
    {
        // Arrange
        UserEntity user = await CreateAndAuthenticateUser();
        string originalPasswordHash = user.Password;
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(
            username: maliciousUsername,
            currentPassword: "TestPass123!",
            newPassword: "NewPass123!",
            newPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        // The malicious username is queried by GetByUsernameAsync, so it must never be executed against the database;
        // if a boolean-injection regression returned the only user in the database, the current password would match and
        // the password would change, so the response must be a failure and the stored password hash must be unchanged.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        UserEntity? storedUser = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == user.Id);
        Assert.NotNull(storedUser);
        Assert.Equal(originalPasswordHash, storedUser!.Password);
    }

    /// <summary>
    /// Asserts that the given <paramref name="content"/> is a problem detail with the expected title and detail.
    /// </summary>
    /// <param name="content">The response content to assert.</param>
    /// <param name="expectedTitle">The expected title of the problem detail.</param>
    /// <param name="expectedDetail">The expected detail of the problem detail.</param>
    private void AssertProblemDetail(string content, string expectedTitle, string expectedDetail)
    {
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(expectedTitle, problemDetails!["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
    }

    /// <summary>
    /// Creates a test user, authenticates it on the client, and returns it.
    /// </summary>
    /// <returns>The created user entity.</returns>
    private async Task<UserEntity> CreateAndAuthenticateUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();

        // Create test user.
        UserEntity user = _userEntityFixture.Create(username: _testUsername, password: _hashService.HashString("TestPass123!"));
        user.TotpSecret = null;

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Authenticate user.
        LoginRequest loginRequest = _loginRequestFixture.Create(
            username: user.Username,
            password: "TestPass123!",
            includeTotpCode: false
        );

        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        // Set auth header for subsequent requests.
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

        return user;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
        }

        _client.DefaultRequestHeaders.Authorization = null;
    }
}
