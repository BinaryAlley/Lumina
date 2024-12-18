#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
            .Include(user => user.UserRoles)
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        AuthorizationResponse? result = JsonSerializer.Deserialize<AuthorizationResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.Roles.Should().Contain("Admin");
        result.Permissions.Should().Contain(AuthorizationPermission.CanViewUsers);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ShouldReturnForbidden()
    {
        // Arrange
        GetAuthorizationRequest request = new(Guid.NewGuid());

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["title"].GetString().Should().Be("General.Unauthorized");
        problemDetails["detail"].GetString().Should().Be("NotAuthorized");
        problemDetails["instance"].GetString().Should().Be("/api/v1/auth/get-authorization");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        GetAuthorizationRequest request = new(Guid.Empty);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["detail"].GetString().Should().Be("OneOrMoreValidationErrorsOccurred");
        problemDetails["instance"].GetString().Should().Be("/api/v1/auth/get-authorization");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().Contain("UserIdCannotBeEmpty");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        GetAuthorizationRequest request = new(Guid.NewGuid());
        CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel();
            await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    private static async Task<UserEntity> AddRolesAndPermissionsToUser(UserEntity user, LuminaDbContext dbContext)
    {
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

        RolePermissionEntity rolePermission = new()
        {
            Id = Guid.NewGuid(),
            Permission = viewUsersPermission,
            PermissionId = viewUsersPermission.Id,
            Role = adminRole,
            RoleId = adminRole.Id,
            CreatedBy = user.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

        UserRoleEntity userRole = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            RoleId = adminRole.Id,
            Role = adminRole,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };


        dbContext.Roles.Add(adminRole);
        dbContext.Permissions.Add(viewUsersPermission);
        dbContext.RolePermissions.Add(rolePermission);
        user.UserRoles.Add(userRole);
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
