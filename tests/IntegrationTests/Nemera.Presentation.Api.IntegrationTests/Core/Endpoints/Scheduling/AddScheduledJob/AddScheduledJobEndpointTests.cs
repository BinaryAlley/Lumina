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

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Scheduling.AddScheduledJob;

/// <summary>
/// Contains integration tests for the <see cref="AddScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddScheduledJobEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task AddScheduledJob_WhenCalledWithValidIntervalRequest_ShouldCreateScheduledJob()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create(
            name: $"Scan job {Guid.NewGuid()}",
            taskType: ScheduledTaskType.ScanMediaLibraries,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 60);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        ScheduledJobResponse? result = JsonSerializer.Deserialize<ScheduledJobResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(request.Name, result!.Name);
        Assert.Equal(request.TaskType, result.TaskType);
        Assert.Equal(ScheduleType.WithIntervalInMinutes, result.ScheduleType);
        Assert.Equal(60, result.IntervalMinutes);
        Assert.Equal(ScheduledJobStatus.Added, result.Status);
        _createdScheduledJobIds.Add(result.Id);
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledWithValidDailyRequest_ShouldCreateScheduledJob()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create(
            name: $"Daily job {Guid.NewGuid()}",
            taskType: ScheduledTaskType.CleanTemporaryFiles,
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 6,
            minute: 30);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        ScheduledJobResponse? result = JsonSerializer.Deserialize<ScheduledJobResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(ScheduleType.DailyAtHourAndMinute, result!.ScheduleType);
        Assert.Equal(6, result.Hour);
        Assert.Equal(30, result.Minute);
        Assert.Null(result.IntervalMinutes);
        _createdScheduledJobIds.Add(result.Id);
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledWithInvalidName_ShouldReturnValidationProblem()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create(
            name: "   ",
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 60);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create(scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
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
