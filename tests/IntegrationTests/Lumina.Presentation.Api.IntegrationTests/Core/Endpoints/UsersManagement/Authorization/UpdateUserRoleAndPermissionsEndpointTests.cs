#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Authorization;

/// <summary>
/// Contains integration tests for the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateUserRoleAndPermissionsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedAdminClientAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldUpdateUserRoleAndPermissions()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // create a non-admin role first
        RoleEntity editorRole = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "Editor",
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };
        dbContext.Roles.Add(editorRole);
        await dbContext.SaveChangesAsync();

        // create test user
        UserEntity user = new()
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Password = "HashedPassword",
            UserRole = null,
            UserPermissions = [],
            Libraries = [],
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        PermissionEntity[] permissions = [.. dbContext.Permissions.Take(2)];

        UpdateUserRoleAndPermissionsRequest request = new(
            UserId: user.Id,
            RoleId: editorRole.Id,
            Permissions: permissions.Select(p => p.Id).ToList()
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{user.Id}/role-and-permissions", request);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        JsonNode? jsonNode = JsonNode.Parse(content);

        jsonNode.Should().NotBeNull();
        Guid.Parse(jsonNode!["userId"]!.ToString()).Should().Be(user.Id);
        jsonNode["role"]!.ToString().Should().Be("Editor");
        jsonNode["permissions"]!.AsArray().Count.Should().BeGreaterThan(0);
    }


    [Fact]
    public async Task ExecuteAsync_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        UpdateUserRoleAndPermissionsRequest request = new(
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            Permissions: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{request.UserId}/role-and-permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["title"].GetString().Should().Be("General.Unauthorized");
        problemDetails["detail"].GetString().Should().Be("NotAuthorized");
        problemDetails["instance"].GetString().Should().Be($"/api/v1/auth/users/{request.UserId}/role-and-permissions");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTryingToRemoveLastAdmin_ShouldReturnForbiddenResult()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // create Editor role
        RoleEntity editorRole = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "Editor",
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };
        dbContext.Roles.Add(editorRole);
        await dbContext.SaveChangesAsync();

        // get the admin user that was created by CreateAuthenticatedAdminClientAsync
        UserEntity? adminUser = dbContext.Users.FirstOrDefault(u => u.UserRole!.Role.RoleName == "Admin");
        adminUser.Should().NotBeNull("Admin user should exist");

        UpdateUserRoleAndPermissionsRequest request = new(
            UserId: adminUser!.Id,
            RoleId: editorRole.Id,
            Permissions: []
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{request.UserId}/role-and-permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["title"].GetString().Should().Be("General.Forbidden");
        problemDetails["detail"].GetString().Should().Be("CannotRemoveLastAdmin");
        problemDetails["instance"].GetString().Should().Be($"/api/v1/auth/users/{request.UserId}/role-and-permissions");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid nonExistentUserId = Guid.NewGuid();
        UpdateUserRoleAndPermissionsRequest request = new(
            UserId: nonExistentUserId,
            RoleId: Guid.NewGuid(),
            Permissions: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{nonExistentUserId}/role-and-permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status404NotFound);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails["title"].GetString().Should().Be("General.NotFound");
        problemDetails["detail"].GetString().Should().Be("UserDoesNotExist");
        problemDetails["instance"].GetString().Should().Be($"/api/v1/auth/users/{nonExistentUserId}/role-and-permissions");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        UpdateUserRoleAndPermissionsRequest request = new(
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            Permissions: [Guid.NewGuid()]
        );

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel();
            await _client.PutAsJsonAsync($"/api/v1/auth/users/{request.UserId}/role-and-permissions", request, cts.Token);
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

        // clear junction tables first
        await dbContext.Set<RolePermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserPermissionEntity>().ExecuteDeleteAsync();

        // then clear main tables
        await dbContext.Set<UserEntity>().ExecuteDeleteAsync();
        await dbContext.Set<RoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<PermissionEntity>().ExecuteDeleteAsync();

        await dbContext.SaveChangesAsync();

        _apiFactory.Dispose();
    }
}
