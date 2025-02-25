#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.IntegrationTests.Common.Converters;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Authorization;

/// <summary>
/// Contains integration tests for the <see cref="GetAuthorizationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    private HttpClient _client;
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetAuthorizationEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
        _testUsername = $"testuser_{Guid.NewGuid()}";
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = 
            { 
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), 
                new ReadOnlySetConverter<AuthorizationPermission>(),
                new ReadOnlySetConverter<string>()
            }
        };
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldReturnAuthorizationDetails()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = await dbContext.Users
            .Include(user => user.UserRole)
                .ThenInclude(userRole => userRole.Role)
            .Include(user => user.UserPermissions)
                .ThenInclude(userPermission => userPermission.Permission)
            .FirstAsync(user => user.Username == _apiFactory.TestUsername);
        await AddRolesAndPermissionsToUser(user, dbContext);
        GetAuthorizationRequest request = new(user.Id);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        AuthorizationResponse? result = JsonSerializer.Deserialize<AuthorizationResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.UserId);
        Assert.Contains("Admin", result.Role);
        Assert.Contains(AuthorizationPermission.CanViewUsers, result.Permissions);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ShouldReturnForbiddenResult()
    {
        // Arrange
        GetAuthorizationRequest request = new(Guid.NewGuid());

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/get-authorization", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        GetAuthorizationRequest request = new(Guid.Empty);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/get-authorization", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("UserIdCannotBeEmpty", errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        GetAuthorizationRequest request = new(Guid.NewGuid());
        CancellationTokenSource cts = new();

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            cts.Cancel();
            await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}", cts.Token);
        });
        Assert.IsType<TaskCanceledException>(exception);
    }

    private static async Task<UserEntity> AddRolesAndPermissionsToUser(UserEntity existingUser, LuminaDbContext dbContext)
    {
        // create and save role and permission first
        RoleEntity adminRole = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "Admin",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        PermissionEntity viewUsersPermission = new()
        {
            Id = Guid.NewGuid(),
            PermissionName = AuthorizationPermission.CanViewUsers,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        await dbContext.Roles.AddAsync(adminRole);
        await dbContext.Permissions.AddAsync(viewUsersPermission);
        await dbContext.SaveChangesAsync();

        // create role permission
        RolePermissionEntity rolePermission = new()
        {
            Id = Guid.NewGuid(),
            Permission = viewUsersPermission,
            PermissionId = viewUsersPermission.Id,
            Role = adminRole,
            RoleId = adminRole.Id,
            CreatedBy = existingUser.Id,
            CreatedOnUtc = DateTime.UtcNow
        };
        await dbContext.RolePermissions.AddAsync(rolePermission);
        await dbContext.SaveChangesAsync();

        // remove existing user role if any
        if (existingUser.UserRole != null)
            dbContext.UserRoles.Remove(existingUser.UserRole);

        // add new user role
        UserRoleEntity userRole = new()
        {
            Id = Guid.NewGuid(),
            User = existingUser,
            UserId = existingUser.Id,
            Role = adminRole,
            RoleId = adminRole.Id,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        await dbContext.UserRoles.AddAsync(userRole);
        await dbContext.SaveChangesAsync();

        // refresh the user entity
        await dbContext.Entry(existingUser).ReloadAsync();

        return existingUser;
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
