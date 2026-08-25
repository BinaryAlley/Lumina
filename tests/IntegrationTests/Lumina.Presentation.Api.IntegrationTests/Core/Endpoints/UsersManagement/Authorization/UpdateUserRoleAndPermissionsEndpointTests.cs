#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.DataAccess.Core.UoW;
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
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly UpdateUserRoleAndPermissionsRequestFixture _updateUserRoleAndPermissionsRequestFixture = new();
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
    public async Task UpdateUserRoleAndPermissions_WhenCalledWithValidRequest_ShouldUpdateUserRoleAndPermissions()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // create a non-admin role first
        RoleEntity editorRole = _roleEntityFixture.Create(roleName: "Editor");
        dbContext.Roles.Add(editorRole);
        await dbContext.SaveChangesAsync();

        // create test user
        UserEntity user = _userEntityFixture.Create(username: "TestUser", password: "HashedPassword");
        user.TotpSecret = null;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        PermissionEntity[] permissions = [.. dbContext.Permissions.Take(2)];

        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: user.Id,
            roleId: editorRole.Id,
            permissions: permissions.Select(p => p.Id).ToList()
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{user.Id}/role-and-permissions", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        JsonNode? jsonNode = JsonNode.Parse(content);

        Assert.NotNull(jsonNode);
        Assert.Equal(user.Id, Guid.Parse(jsonNode!["userId"]!.ToString()));
        Assert.Equal("Editor", jsonNode["role"]!.ToString());
        Assert.True(jsonNode["permissions"]!.AsArray().Count > 0);
    }


    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: Guid.NewGuid(),
            roleId: Guid.NewGuid(),
            permissions: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{request.UserId}/role-and-permissions", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/auth/users/{request.UserId}/role-and-permissions", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenTryingToRemoveLastAdmin_ShouldReturnForbiddenResult()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // create Editor role
        RoleEntity editorRole = _roleEntityFixture.Create(roleName: "Editor");
        dbContext.Roles.Add(editorRole);
        await dbContext.SaveChangesAsync();

        // get the admin user that was created by CreateAuthenticatedAdminClientAsync
        UserEntity? adminUser = dbContext.Users.FirstOrDefault(u => u.UserRole!.Role.RoleName == "Admin");
        Assert.NotNull(adminUser);
        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: adminUser!.Id,
            roleId: editorRole.Id,
            permissions: []
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{request.UserId}/role-and-permissions", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Forbidden", problemDetails["title"].GetString());
        Assert.Equal("CannotRemoveLastAdmin", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/auth/users/{request.UserId}/role-and-permissions", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid nonExistentUserId = Guid.NewGuid();
        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: nonExistentUserId,
            roleId: Guid.NewGuid(),
            permissions: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/users/{nonExistentUserId}/role-and-permissions", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("UserDoesNotExist", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/auth/users/{nonExistentUserId}/role-and-permissions", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: Guid.NewGuid(),
            roleId: Guid.NewGuid(),
            permissions: [Guid.NewGuid()]
        );

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            cts.Cancel();
            await _client.PutAsJsonAsync($"/api/v1/auth/users/{request.UserId}/role-and-permissions", request, cts.Token);
        });
        Assert.IsType<TaskCanceledException>(exception);
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

        await _apiFactory.RemoveTestUserAsync();
    }
}
