#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Contains integration tests for the <see cref="CheckInitializationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="CheckInitializationEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CheckInitializationEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenNoUsersExist_ShouldReturnNotInitialized()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/initialization");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        InitializationResponse? result = JsonSerializer.Deserialize<InitializationResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUsersExist_ShouldReturnInitialized()
    {
        // Arrange
        await CreateTestUser();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/initialization");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        InitializationResponse? result = JsonSerializer.Deserialize<InitializationResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel();
            await _client.GetAsync("/api/v1/initialization", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
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
    public Task DisposeAsync()
    {
        _apiFactory.Dispose();
        return Task.CompletedTask;
    }
}
