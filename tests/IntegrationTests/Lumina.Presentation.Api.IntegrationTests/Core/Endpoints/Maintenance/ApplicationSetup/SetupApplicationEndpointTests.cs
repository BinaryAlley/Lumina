#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Infrastructure.Core.Security;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Contains integration tests for the <see cref="SetupApplicationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="SetupApplicationEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetupApplicationEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidRequestAnd2FA_ShouldSetupApplicationsuccessfuly()
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!",
            Use2fa: true
        );
        await _apiFactory.ResetDatabaseAsync();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/initialization", request);

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

        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"http://localhost/api/v1/users/{result.Id}", response.Headers.Location.ToString());

        // verify default permissions, roles, and role permissions are set
        await VerifyDefaultSetup(result.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequestWithout2FA_ShouldSetupApplicationsuccessfuly()
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!",
            Use2fa: false
        );
        await _apiFactory.ResetDatabaseAsync();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/initialization", request);

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

        // verify default permissions, roles, and role permissions are set
        await VerifyDefaultSetup(result.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAdminAccountAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        await CreateTestUser();
        RegistrationRequest request = new(
            Username: "another_admin",
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!",
            Use2fa: false
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/initialization", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.10", problemDetails["type"].GetString());
        Assert.Equal("General.Conflict", problemDetails["title"].GetString());
        Assert.Equal("AdminAccountAlreadyCreated", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/initialization", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNull_ShouldReturnValidationError()
    {
        // Arrange
        RegistrationRequest? request = null;

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/initialization", request);

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

    private async Task VerifyDefaultSetup(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        
        // TODO: update list when defaults change
        // verify default permissions
        List<PermissionEntity> permissions = await dbContext.Permissions.ToListAsync();
        Assert.Contains(permissions, p => p.PermissionName == AuthorizationPermission.CanViewUsers);
        Assert.Contains(permissions, p => p.PermissionName == AuthorizationPermission.CanDeleteUsers);
        Assert.Contains(permissions, p => p.PermissionName == AuthorizationPermission.CanRegisterUsers);

        // verify default roles
        List<RoleEntity> roles = await dbContext.Roles.ToListAsync();
        RoleEntity? adminRole = roles.FirstOrDefault(r => r.RoleName == "Admin");
        Assert.NotNull(adminRole);

        // verify admin role permissions
        List<RolePermissionEntity> rolePermissions = await dbContext.RolePermissions
            .Where(rp => rp.RoleId == adminRole!.Id)
            .ToListAsync();
        Assert.NotEmpty(rolePermissions);
        Assert.Contains(rolePermissions, rp => permissions.Any(p => p.Id == rp.PermissionId));

        // verify user has admin role
        UserRoleEntity? userRole = await dbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == adminRole!.Id);
        Assert.NotNull(userRole);
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
            UserRole = null, // TODO: test default role and permissions?
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
