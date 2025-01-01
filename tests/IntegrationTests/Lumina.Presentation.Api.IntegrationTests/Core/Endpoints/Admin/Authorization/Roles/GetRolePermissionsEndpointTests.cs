#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Admin.Authorization.Roles;

/// <summary>
/// Contains integration tests for the <see cref="GetRolePermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetRolePermissionsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidRequest_ShouldReturnRolePermissions()
    {
        // Arrange
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        RoleEntity? role = dbContext.Roles.FirstOrDefault();
        role.Should().NotBeNull("Admin role should exist");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/roles/{role!.Id}/permissions");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        RolePermissionsResponse? result = JsonSerializer.Deserialize<RolePermissionsResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Role.Id.Should().Be(role.Id);
        result.Role.RoleName.Should().Be("Admin");
        result.Permissions.Should().NotBeEmpty();
        result.Permissions.Should().AllSatisfy(permission =>
        {
            permission.Id.Should().NotBe(Guid.Empty);
            permission.PermissionName.Should().NotBe(AuthorizationPermission.None);
        });
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync(); // authenticated user, but not admin
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        RoleEntity? role = dbContext.Roles.FirstOrDefault();
        role.Should().NotBeNull("Admin role should exist");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/roles/{role!.Id}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["title"].GetString().Should().Be("General.Unauthorized");
        problemDetails["detail"].GetString().Should().Be("NotAuthorized");
        problemDetails["instance"].GetString().Should().Be($"/api/v1/roles/{role.Id}/permissions");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidRoleId_ShouldReturnNotFound()
    {
        // Act
        Guid invalidRoleId = Guid.NewGuid();
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/roles/{invalidRoleId}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status404NotFound);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails["title"].GetString().Should().Be("General.NotFound");
        problemDetails["detail"].GetString().Should().Be("RoleNotFound");
        problemDetails["instance"].GetString().Should().Be($"/api/v1/roles/{invalidRoleId}/permissions");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        RoleEntity? role = dbContext.Roles.FirstOrDefault();
        role.Should().NotBeNull("Admin role should exist");

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel();
            await _client.GetAsync($"/api/v1/roles/{role!.Id}/permissions", cts.Token);
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
