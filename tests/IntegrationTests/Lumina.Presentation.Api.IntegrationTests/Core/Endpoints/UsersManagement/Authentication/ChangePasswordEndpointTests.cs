#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains integration tests for the <see cref="ChangePasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ChangePasswordEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldChangePasswordSuccessfully()
    {
        // Arrange
        ChangePasswordRequest request = new(
            Username: _apiFactory.TestUsername,
            CurrentPassword: "TestPass123!",
            NewPassword: "NewPass123!",
            NewPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        string content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ChangePasswordResponse? result = JsonSerializer.Deserialize<ChangePasswordResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.IsPasswordChanged.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordsDoNotMatch_ShouldReturnValidationError()
    {
        // Arrange
        ChangePasswordRequest request = new(
            Username: _apiFactory.TestUsername,
            CurrentPassword: "OldPass123!",
            NewPassword: "NewPass123!",
            NewPasswordConfirm: "DifferentPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["title"].GetString().Should().Be("Validation Error");
        problemDetails["detail"].GetString().Should().Be("One or more validation errors occurred.");
        problemDetails["instance"].GetString().Should().Be("/api/v1/auth/change-password");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().Contain("PasswordsNotMatch");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        ChangePasswordRequest request = new(
            Username: "nonexistentuser",
            CurrentPassword: "OldPass123!",
            NewPassword: "NewPass123!",
            NewPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status404NotFound);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails["title"].GetString().Should().Be("General.NotFound");
        problemDetails["detail"].GetString().Should().Be("UsernameDoesNotExist");
        problemDetails["instance"].GetString().Should().Be("/api/v1/auth/change-password");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrentPasswordIsIncorrect_ShouldReturnForbidden()
    {
        // Arrange
        ChangePasswordRequest request = new(
            Username: _apiFactory.TestUsername,
            CurrentPassword: "WrongPass123!",
            NewPassword: "NewPass123!",
            NewPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["title"].GetString().Should().Be("General.Failure");
        problemDetails["detail"].GetString().Should().Be("InvalidCurrentPassword");
        problemDetails["instance"].GetString().Should().Be("/api/v1/auth/change-password");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNull_ShouldReturnValidationError()
    {
        // Arrange
        ChangePasswordRequest? request = null;

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["title"].GetString().Should().Be("Validation Error");
        problemDetails["detail"].GetString().Should().Be("One or more validation errors occurred.");
        problemDetails["instance"].GetString().Should().Be("/api/v1/auth/change-password");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().Contain(["UsernameCannotBeEmpty", "CurrentPasswordCannotBeEmpty", "NewPasswordCannotBeEmpty", "NewPasswordConfirmCannotBeEmpty"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        ChangePasswordRequest request = new(
            Username: _apiFactory.TestUsername,
            CurrentPassword: "OldPass123!",
            NewPassword: "NewPass123!",
            NewPasswordConfirm: "NewPass123!"
        );
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel();
            await _client.PostAsJsonAsync("/api/v1/auth/change-password", request, cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _apiFactory.TestUsername!);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }
}
