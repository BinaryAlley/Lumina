#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Scheduling.GetScheduledJobs;

/// <summary>
/// Contains integration tests for the <see cref="GetScheduledJobsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobsEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private readonly AuthenticatedNemeraApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly AddScheduledJobRequestFixture _addScheduledJobRequestFixture = new();
    private readonly List<Guid> _createdScheduledJobIds = [];
    private HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetScheduledJobsEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task GetScheduledJobs_WhenScheduledJobsExist_ShouldReturnTheListOfScheduledJobs()
    {
        // Arrange
        AddScheduledJobRequest addRequest = _addScheduledJobRequestFixture.Create(
            name: $"Listed job {Guid.NewGuid()}",
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 30);
        HttpResponseMessage addResponse = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", addRequest);
        string addContent = await addResponse.Content.ReadAsStringAsync();
        ScheduledJobResponse? createdJob = JsonSerializer.Deserialize<ScheduledJobResponse>(addContent, _jsonOptions);
        Assert.NotNull(createdJob);
        _createdScheduledJobIds.Add(createdJob!.Id);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/scheduled-jobs");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        List<ScheduledJobResponse>? result = JsonSerializer.Deserialize<List<ScheduledJobResponse>>(content, _jsonOptions);
        Assert.NotNull(result);
        Assert.Contains(result!, scheduledJob => scheduledJob.Id == createdJob.Id && scheduledJob.Name == addRequest.Name);
    }

    [Fact]
    public async Task GetScheduledJobs_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/scheduled-jobs");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
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
