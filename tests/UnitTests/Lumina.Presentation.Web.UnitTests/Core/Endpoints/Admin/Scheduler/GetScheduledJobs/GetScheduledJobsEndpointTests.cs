#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobs;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.GetScheduledJobs;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetScheduledJobsEndpoint _sut;
    private readonly ScheduledJobDtoFixture _scheduledJobDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsEndpointTests"/> class.
    /// </summary>
    public GetScheduledJobsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetScheduledJobsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsJobs_ShouldReturnSuccessJsonWithScheduledJobs()
    {
        // Arrange
        ScheduledJobDto[] expectedJobs = [.. _scheduledJobDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<ScheduledJobDto[]>(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOBS, Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        ScheduledJobDto[]? returnedJobs = jsonDocument.RootElement.GetProperty("data").Deserialize<ScheduledJobDto[]>(_jsonOptions);
        Assert.Equal(expectedJobs.Length, returnedJobs!.Length);
        Assert.Equal(expectedJobs.Select(job => job.Name), returnedJobs.Select(job => job.Name));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestScheduledJobsFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<ScheduledJobDto[]>(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOBS, Arg.Any<CancellationToken>())
            .Returns([.. _scheduledJobDtoFixture.CreateMany()]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ScheduledJobDto[]>(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOBS, Arg.Any<CancellationToken>());
    }
}
