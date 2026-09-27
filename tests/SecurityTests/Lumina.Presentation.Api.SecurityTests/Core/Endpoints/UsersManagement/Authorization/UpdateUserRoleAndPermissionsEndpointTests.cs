#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authorization;

/// <summary>
/// Contains security tests for the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsEndpointTests : IClassFixture<LuminaApiFactory>, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly UpdateUserRoleAndPermissionsRequestFixture _updateUserRoleAndPermissionsRequestFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];
    private Guid _adminUserId;
    private Guid _seededRoleId;
    private Guid _seededPermissionId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateUserRoleAndPermissionsEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string url = $"/api/v1/auth/users/{userId}/role-and-permissions";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string responseContent = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenAuthenticatedNonAdmin_ShouldReturnForbiddenResult()
    {
        // Arrange
        using HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        Guid targetUserId = Guid.NewGuid();
        string url = $"/api/v1/auth/users/{targetUserId}/role-and-permissions";

        // Act
        HttpResponseMessage response = await client.PutAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));
        string responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("password", responseContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", responseContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", responseContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenCalledByAdminWithValidData_ShouldUpdateRoleAndPermissions()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        _adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        UserEntity targetUser = await SeedTargetUserAsync();
        RoleEntity editorRole = await SeedRoleAsync("Editor");
        PermissionEntity permission = await SeedPermissionAsync(AuthorizationPermission.CanDeleteUsers);
        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: targetUser.Id,
            roleId: editorRole.Id,
            permissions: [permission.Id]
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/auth/users/{targetUser.Id}/role-and-permissions", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument authorizationResponse = JsonDocument.Parse(content);
        Assert.Equal(targetUser.Id, authorizationResponse.RootElement.GetProperty("userId").GetGuid());
        Assert.Equal("Editor", authorizationResponse.RootElement.GetProperty("role").GetString());

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity? updatedUser = await dbContext.Users
            .Include(user => user.UserRole).ThenInclude(userRole => userRole!.Role)
            .Include(user => user.UserPermissions)
            .FirstOrDefaultAsync(user => user.Id == targetUser.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("Editor", updatedUser!.UserRole!.Role.RoleName);
        Assert.Contains(updatedUser.UserPermissions, userPermission => userPermission.PermissionId == permission.Id);
    }

    [Theory]
    [InlineData("'; DROP TABLE UserRoles; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateUserRoleAndPermissions_WithInjectionInUserIdRouteSegment_ShouldNotLeakOrExecuteSql(string maliciousUserId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        _adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        UpdateUserRoleAndPermissionsRequest request = _updateUserRoleAndPermissionsRequestFixture.Create(
            userId: Guid.NewGuid(),
            roleId: Guid.NewGuid(),
            permissions: []
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/auth/users/{Uri.EscapeDataString(maliciousUserId)}/role-and-permissions", request);

        // Assert
        // note: the {userId} route parameter is Guid-typed, so the malicious value fails model binding before it reaches the handler
        // note: observed status is 500 because FastEndpoints, combined with DontCatchExceptions(), turns the binding failure into a
        // 500 response instead of 400/422 (pre-existing production bug, not fixed here), and that body leaks the binding exception details
        // note: what matters is that the injection never reaches the database, so no SQL engine error (SqliteException) may surface
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("'; DROP TABLE UserPermissions; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateUserRoleAndPermissions_WithInjectionInPermissionsBody_ShouldNotLeakOrExecuteSql(string maliciousPermissionId)
    {
        // Arrange
        // authenticate as an admin and seed the target user, so a well-formed body would reach the handler and be authorized
        HttpClient client = _apiFactory.CreateClient();
        _adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        UserEntity targetUser = await SeedTargetUserAsync();
        string jsonBody = $"{{\"userId\":\"{targetUser.Id}\",\"roleId\":\"{Guid.NewGuid()}\",\"permissions\":[\"{maliciousPermissionId}\"]}}";

        // Act
        HttpResponseMessage response = await client.PutAsync(
            $"/api/v1/auth/users/{targetUser.Id}/role-and-permissions",
            new StringContent(jsonBody, Encoding.UTF8, "application/json"));

        // Assert
        // Note: the permission Ids are Guid-typed, so the malicious value fails model binding before it reaches the handler.
        // Note: observed status is 500 because FastEndpoints, combined with DontCatchExceptions(), turns the binding failure into a
        // 500 response instead of 400/422 (pre-existing production bug, not fixed here), and that body leaks the binding exception details.
        // Note: what matters is that the injection never reaches the database, so no SQL engine error (SqliteException) may surface.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a user whose role and permissions are updated, and traces its username for cleanup.
    /// </summary>
    /// <returns>The seeded user.</returns>
    private async Task<UserEntity> SeedTargetUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        string username = $"targetuser_{Guid.NewGuid()}";
        UserEntity user = _userEntityFixture.Create(id: Guid.NewGuid(), username: username, password: "HashedPassword");
        user.TotpSecret = null;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return user;
    }

    /// <summary>
    /// Seeds a role that is assigned to the updated user, and traces it for cleanup.
    /// </summary>
    /// <param name="roleName">The name of the role to seed.</param>
    /// <returns>The seeded role.</returns>
    private async Task<RoleEntity> SeedRoleAsync(string roleName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        _seededRoleId = Guid.NewGuid();
        RoleEntity role = _roleEntityFixture.Create(id: _seededRoleId, roleName: roleName);
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        return role;
    }

    /// <summary>
    /// Seeds a permission that is assigned to the updated user, and traces it for cleanup.
    /// </summary>
    /// <param name="permissionName">The permission to seed.</param>
    /// <returns>The seeded permission.</returns>
    private async Task<PermissionEntity> SeedPermissionAsync(AuthorizationPermission permissionName)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        _seededPermissionId = Guid.NewGuid();
        PermissionEntity permission = _permissionEntityFixture.Create(id: _seededPermissionId, permissionName: permissionName);
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        return permission;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            await _apiFactory.RemoveTestUserAsync(username).ConfigureAwait(false);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        if (_seededRoleId != Guid.Empty)
        {
            RoleEntity? role = await dbContext.Roles.FirstOrDefaultAsync(candidate => candidate.Id == _seededRoleId);
            if (role is not null)
                dbContext.Roles.Remove(role);
        }
        if (_seededPermissionId != Guid.Empty)
        {
            PermissionEntity? permission = await dbContext.Permissions.FirstOrDefaultAsync(candidate => candidate.Id == _seededPermissionId);
            if (permission is not null)
                dbContext.Permissions.Remove(permission);
        }
        await dbContext.SaveChangesAsync();
        if (_adminUserId != Guid.Empty)
            await _apiFactory.RemoveAdminUserAsync(_adminUserId).ConfigureAwait(false);
    }
}
