#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Scheduling.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains integration tests for the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly UpdateSchedulerDisplayPreferencesRequestFixture _updateSchedulerDisplayPreferencesRequestFixture = new();
    private Guid? _createdPreferencesUserId;
    private HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateSchedulerDisplayPreferencesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledWithValidRequest_ShouldPersistThePreferences()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create(
            jobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            displayTimeSpan: 30,
            displayTimeUnit: SchedulerDisplayTimeUnit.Minutes);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/scheduled-jobs/display-preferences", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        HttpResponseMessage getResponse = await _client.GetAsync("/api/v1/scheduled-jobs/display-preferences");
        string content = await getResponse.Content.ReadAsStringAsync();
        SchedulerDisplayPreferencesResponse? result = JsonSerializer.Deserialize<SchedulerDisplayPreferencesResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(ScheduledTaskType.ScanMediaLibraries, result!.JobTypeFilter);
        Assert.Equal(30, result.DisplayTimeSpan);
        Assert.Equal(SchedulerDisplayTimeUnit.Minutes, result.DisplayTimeUnit);
        _createdPreferencesUserId = result.UserId;
    }

    [Fact]
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledWithNonPositiveTimeSpan_ShouldReturnValidationProblem()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create(displayTimeSpan: 0);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/scheduled-jobs/display-preferences", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
    }

    [Fact]
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/scheduled-jobs/display-preferences", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

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
