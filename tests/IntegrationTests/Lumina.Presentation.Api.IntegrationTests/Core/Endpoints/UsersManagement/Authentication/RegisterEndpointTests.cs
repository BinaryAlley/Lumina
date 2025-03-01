#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains integration tests for the <see cref="RegisterEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RegisterEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = _apiFactory.CreateClient();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldRegisterUsersuccessfuly()
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!",
            Use2fa: false
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        RegistrationResponse? result = JsonSerializer.Deserialize<RegistrationResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(_testUsername, result!.Username);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Null(result.TotpSecret);

        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"http://localhost/api/v1/users/{result.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequestAnd2FA_ShouldRegisterUserWithTotp()
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!",
            Use2fa: true
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        RegistrationResponse? result = JsonSerializer.Deserialize<RegistrationResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(_testUsername, result!.Username);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.TotpSecret);
        Assert.NotEmpty(result.TotpSecret);
        Assert.StartsWith("data:image/png;base64,", result.TotpSecret);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUsernameAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        await CreateTestUser();
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!",
            Use2fa: false
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Conflict", problemDetails["title"].GetString());
        Assert.Equal("UsernameAlreadyExists", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordsDoNotMatch_ShouldReturnValidationError()
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "DifferentPass123!",
            Use2fa: false
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Dictionary<string, string[]>? errors = problemDetails!["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("PasswordsNotMatch", errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNull_ShouldReturnValidationError()
    {
        // Arrange
        RegistrationRequest? request = null;

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Dictionary<string, string[]>? errors = problemDetails!["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("UsernameCannotBeEmpty", errors["General.Validation"]);
        Assert.Contains("PasswordCannotBeEmpty", errors["General.Validation"]);
        Assert.Contains("PasswordConfirmCannotBeEmpty", errors["General.Validation"]);
    }

    private async Task<UserEntity> CreateTestUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = new()
        {
            Username = _testUsername,
            Password = new PasswordHashService().HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }
}
