#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Admin.Authorization.Roles;

/// <summary>
/// Contains security tests for the <c>/auth/roles</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointTests : IClassFixture<LuminaApiFactory>, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private Guid _adminUserId;
    private Guid _seededPermissionId;
    private Guid _seededRoleId;
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly UpdateRoleRequestFixture _updateRoleRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateRoleEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateRole_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        UpdateRoleRequest requestBody = _updateRoleRequestFixture.Create(
            roleId: Guid.NewGuid(),
            roleName: "Editor",
            permissions: []
        );

        StringContent content = new(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/v1/auth/roles", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string responseContent = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/auth/roles", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Roles--")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateRole_WithSQLInjectionInRoleName_ShouldNotCorruptOrDeleteData(string maliciousRoleName)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        _adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        _seededPermissionId = Guid.NewGuid();
        _seededRoleId = Guid.NewGuid();
        RoleEntity seededRole = _roleEntityFixture.Create(id: _seededRoleId, roleName: "ExistingRole");
        using (IServiceScope seedScope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext seedDbContext = seedScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            seedDbContext.Permissions.Add(_permissionEntityFixture.Create(id: _seededPermissionId, permissionName: AuthorizationPermission.CanDeleteUsers));
            seedDbContext.Roles.Add(seededRole);
            await seedDbContext.SaveChangesAsync();
        }
        UpdateRoleRequest requestBody = _updateRoleRequestFixture.Create(
            roleId: seededRole.Id,
            roleName: maliciousRoleName,
            permissions: [_seededPermissionId]
        );
        StringContent content = new(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.PutAsync("/api/v1/auth/roles", content);

        // Assert
        // the malicious role name passes the authenticated handler and the permissions validation, reaches the
        // parameterized update, and is persisted verbatim: if it were concatenated into raw SQL, the update would fail
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        RoleEntity? updatedRole = await dbContext.Roles.FirstOrDefaultAsync(role => role.Id == seededRole.Id);
        Assert.NotNull(updatedRole);
        Assert.Equal(maliciousRoleName, updatedRole!.RoleName);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        if (_seededPermissionId != Guid.Empty)
        {
            PermissionEntity? permission = dbContext.Permissions.FirstOrDefault(candidate => candidate.Id == _seededPermissionId);
            if (permission is not null)
                dbContext.Permissions.Remove(permission);
        }
        if (_seededRoleId != Guid.Empty)
        {
            RoleEntity? role = dbContext.Roles.FirstOrDefault(candidate => candidate.Id == _seededRoleId);
            if (role is not null)
                dbContext.Roles.Remove(role);
        }
        dbContext.SaveChanges();
        if (_adminUserId != Guid.Empty)
            await _apiFactory.RemoveAdminUserAsync(_adminUserId).ConfigureAwait(false);
    }
}
