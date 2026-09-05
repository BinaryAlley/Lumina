#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
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

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Scheduling.AddScheduledJob;

/// <summary>
/// Contains security tests for the <c>/api/v1/scheduled-jobs</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointTests : IClassFixture<LuminaApiFactory>, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private Guid _adminUserId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddScheduledJobEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task AddScheduledJob_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        object requestBody = new
        {
            Name = "Unauthorized job",
            TaskType = ScheduledTaskType.ScanMediaLibraries,
            ScheduleType = ScheduleType.WithIntervalInMinutes,
            IntervalMinutes = 60
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/scheduled-jobs", requestBody);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateUserAsync(client);
        object requestBody = new
        {
            Name = "Non admin job",
            TaskType = ScheduledTaskType.ScanMediaLibraries,
            ScheduleType = ScheduleType.WithIntervalInMinutes,
            IntervalMinutes = 60
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/scheduled-jobs", requestBody);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE ScheduledJobs--")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("<script>alert('xss')</script>")] // script injection
    public async Task AddScheduledJob_WithMaliciousName_ShouldNotLeakDataOrExecuteInjection(string maliciousName)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        _adminUserId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        object requestBody = new
        {
            Name = maliciousName,
            TaskType = ScheduledTaskType.ScanMediaLibraries,
            ScheduleType = ScheduleType.WithIntervalInMinutes,
            IntervalMinutes = 60
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/scheduled-jobs", requestBody);

        // Assert
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        // The malicious name either reaches the parameterized insert verbatim or is rejected by validation; it never corrupts the storage medium.
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.UnprocessableEntity);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            using IServiceScope scope = _apiFactory.Services.CreateScope();
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            Assert.NotNull(await dbContext.ScheduledJobs.FirstOrDefaultAsync(scheduledJob => scheduledJob.Name == maliciousName));
        }
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_adminUserId != Guid.Empty)
        {
            using IServiceScope scope = _apiFactory.Services.CreateScope();
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

            ScheduledJobEntity[] scheduledJobs = await dbContext.ScheduledJobs
                .Where(scheduledJob => scheduledJob.OwnerUserId == _adminUserId)
                .ToArrayAsync();
            foreach (ScheduledJobEntity scheduledJob in scheduledJobs)
            {
                dbContext.ScheduledJobExecutions.RemoveRange(dbContext.ScheduledJobExecutions.Where(execution => execution.ScheduledJobId == scheduledJob.Id));
                dbContext.ScheduledJobs.Remove(scheduledJob);
            }
            await dbContext.SaveChangesAsync();

            await _apiFactory.RemoveAdminUserAsync(_adminUserId);
        }
    }
}
