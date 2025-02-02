#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Authentication;
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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Admin.Authorization.Roles;

/// <summary>
/// Contains integration tests for the <see cref="UpdateRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateRoleEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldUpdateRole()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        RoleEntity role = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "TestRole",
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();

        PermissionEntity[] permissions = [.. dbContext.Permissions.Take(2)];
        UpdateRoleRequest request = new(
            RoleId: role.Id,
            RoleName: "UpdatedRole",
            Permissions: permissions.Select(p => p.Id).ToList()
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/roles", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        RolePermissionsResponse? result = JsonSerializer.Deserialize<RolePermissionsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(role.Id, result!.Role.Id);
        Assert.Equal("UpdatedRole", result.Role.RoleName);
        Assert.Equal(2, result.Permissions.Length);
        foreach (PermissionDto permission in result.Permissions)
        {
            Assert.NotEqual(Guid.Empty, permission.Id);
            Assert.NotEqual(AuthorizationPermission.None, permission.PermissionName);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        UpdateRoleRequest request = new(
            RoleId: Guid.NewGuid(),
            RoleName: "UpdatedRole",
            Permissions: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/roles", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRoleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        UpdateRoleRequest request = new(
            RoleId: nonExistentRoleId,
            RoleName: "UpdatedRole",
            Permissions: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("RoleNotFound", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/roles", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        UpdateRoleRequest request = new(
            RoleId: Guid.NewGuid(),
            RoleName: "UpdatedRole",
            Permissions: [Guid.NewGuid()]
        );

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            cts.Cancel();
            await _client.PutAsJsonAsync($"/api/v1/auth/roles", request, cts.Token);
        });
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
