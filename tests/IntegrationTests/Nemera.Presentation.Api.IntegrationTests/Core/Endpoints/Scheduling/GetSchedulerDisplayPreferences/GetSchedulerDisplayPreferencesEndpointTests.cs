#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.Scheduling;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Scheduling.GetSchedulerDisplayPreferences;

/// <summary>
/// Contains integration tests for the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private readonly AuthenticatedNemeraApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetSchedulerDisplayPreferencesEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes an authenticated admin API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedAdminClientAsync();
    }

    [Fact]
    public async Task GetSchedulerDisplayPreferences_WhenNoPreferencesStored_ShouldReturnTheDefaultPreferences()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/scheduled-jobs/display-preferences");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        SchedulerDisplayPreferencesResponse? result = JsonSerializer.Deserialize<SchedulerDisplayPreferencesResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Null(result!.JobTypeFilter);
        Assert.Equal(10, result.DisplayTimeSpan);
        Assert.Equal(SchedulerDisplayTimeUnit.Minutes, result.DisplayTimeUnit);
    }

    [Fact]
    public async Task GetSchedulerDisplayPreferences_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/scheduled-jobs/display-preferences");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();

        // Wipe the tables touched by the tests, because the in-memory database of the class factory is shared between the tests of the class.
        await dbContext.Set<RolePermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserPermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<ScheduledJobExecutionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<ScheduledJobEntity>().ExecuteDeleteAsync();
        await dbContext.Set<SchedulerDisplayPreferencesEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserRoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<RoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<PermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserEntity>().ExecuteDeleteAsync();

        await dbContext.SaveChangesAsync();
    }
}
