#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.AddScheduledJob;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.AddScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="AddScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly AddScheduledJobEndpoint _sut;
    private readonly AddScheduledJobRequestFixture _addScheduledJobRequestFixture = new();
    private readonly ScheduledJobDtoFixture _scheduledJobDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointTests"/> class.
    /// </summary>
    public AddScheduledJobEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<AddScheduledJobEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsCreatedJob_ShouldReturnSuccessJsonWithCreatedJob()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create();
        ScheduledJobDto expectedJob = _scheduledJobDtoFixture.Create(name: request.Name);
        _mockApiHttpClient.PostAsync<ScheduledJobDto, AddScheduledJobRequest>(ApiRoutes.ScheduledJobs.ADD_SCHEDULED_JOB, Arg.Any<AddScheduledJobRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedJob.Name, jsonDocument.RootElement.GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardRequestToApiAndReturnSuccess()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create();
        _mockApiHttpClient.PostAsync<ScheduledJobDto, AddScheduledJobRequest>(Arg.Any<string>(), Arg.Any<AddScheduledJobRequest>(), Arg.Any<CancellationToken>())
            .Returns(_scheduledJobDtoFixture.Create());

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ScheduledJobDto, AddScheduledJobRequest>(
            ApiRoutes.ScheduledJobs.ADD_SCHEDULED_JOB,
            Arg.Is<AddScheduledJobRequest>(forwardedRequest =>
                forwardedRequest.Name == request.Name &&
                forwardedRequest.TaskType == request.TaskType &&
                forwardedRequest.ScheduleType == request.ScheduleType &&
                forwardedRequest.IntervalMinutes == request.IntervalMinutes &&
                forwardedRequest.Hour == request.Hour &&
                forwardedRequest.Minute == request.Minute),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
