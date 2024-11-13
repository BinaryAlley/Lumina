#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
public class RegisterEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
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
    public RegisterEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldRegisterUserSuccessfully()
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        string content = await response.Content.ReadAsStringAsync();
        RegistrationResponse? result = JsonSerializer.Deserialize<RegistrationResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Username.Should().Be(_testUsername);
        result.Id.Should().NotBe(Guid.Empty);
        result.TotpSecret.Should().BeNull();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"http://localhost/api/v1/users/{result.Id}");
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        string content = await response.Content.ReadAsStringAsync();
        RegistrationResponse? result = JsonSerializer.Deserialize<RegistrationResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Username.Should().Be(_testUsername);
        result.Id.Should().NotBe(Guid.Empty);
        result.TotpSecret.Should().NotBeNullOrEmpty();
        result.TotpSecret.Should().StartWith("data:image/png;base64,");
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
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status409Conflict);
        problemDetails["title"].GetString().Should().Be("General.Conflict");
        problemDetails["detail"].GetString().Should().Be("UsernameAlreadyExists");
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
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        Dictionary<string, string[]>? errors = problemDetails!["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().Contain("PasswordsNotMatch");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNull_ShouldReturnValidationError()
    {
        // Arrange
        RegistrationRequest? request = null;

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        Dictionary<string, string[]>? errors = problemDetails!["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation")
            .WhoseValue.Should().Contain(["UsernameCannotBeEmpty", "PasswordCannotBeEmpty", "PasswordConfirmCannotBeEmpty"]);
    }

    private async Task<UserEntity> CreateTestUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = new()
        {
            Username = _testUsername,
            Password = new HashService().HashString("TestPass123!"),
            Libraries = [],
            Created = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
        }
    }
}
