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

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Scheduling.RemoveScheduledJob;

/// <summary>
/// Contains integration tests for the <see cref="RemoveScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="RemoveScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RemoveScheduledJobEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task RemoveScheduledJob_WhenScheduledJobExists_ShouldRemoveIt()
    {
        // Arrange
        ScheduledJobResponse createdJob = await CreateScheduledJobAsync();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/scheduled-jobs/{createdJob.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Assert.Null(await dbContext.ScheduledJobs.FirstOrDefaultAsync(scheduledJob => scheduledJob.Id == createdJob.Id));
    }

    [Fact]
    public async Task RemoveScheduledJob_WhenScheduledJobDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid missingScheduledJobId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/scheduled-jobs/{missingScheduledJobId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("ScheduledJobNotFound", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task RemoveScheduledJob_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        Guid scheduledJobId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/scheduled-jobs/{scheduledJobId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Creates a scheduled job through the API and returns its response.
    /// </summary>
    /// <returns>The created scheduled job response.</returns>
    private async Task<ScheduledJobResponse> CreateScheduledJobAsync()
    {
        AddScheduledJobRequest addRequest = _addScheduledJobRequestFixture.Create(
            name: $"Removable job {Guid.NewGuid()}",
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 60);
        HttpResponseMessage addResponse = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", addRequest);
        string addContent = await addResponse.Content.ReadAsStringAsync();
        ScheduledJobResponse? createdJob = JsonSerializer.Deserialize<ScheduledJobResponse>(addContent, _jsonOptions);
        Assert.NotNull(createdJob);
        _createdScheduledJobIds.Add(createdJob!.Id);
        return createdJob;
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
