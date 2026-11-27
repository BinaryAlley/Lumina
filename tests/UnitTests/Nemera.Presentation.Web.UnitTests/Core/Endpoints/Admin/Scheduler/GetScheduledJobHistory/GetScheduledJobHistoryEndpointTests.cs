#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobHistory;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.GetScheduledJobHistory;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetScheduledJobHistoryEndpoint _sut;
    private readonly GetScheduledJobHistoryRequestFixture _getScheduledJobHistoryRequestFixture = new();
    private readonly ScheduledJobExecutionDtoFixture _scheduledJobExecutionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpointTests"/> class.
    /// </summary>
    public GetScheduledJobHistoryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetScheduledJobHistoryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsExecutions_ShouldReturnSuccessJsonWithExecutions()
    {
        // Arrange
        ScheduledJobExecutionDto[] expectedExecutions = [.. _scheduledJobExecutionDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<ScheduledJobExecutionDto[]>(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY, Arg.Any<CancellationToken>())
            .Returns(expectedExecutions);

        // Act
        IResult result = await _sut.ExecuteAsync(_getScheduledJobHistoryRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedExecutions.Length, jsonDocument.RootElement.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFromAndToAreNull_ShouldRequestHistoryWithoutQueryString()
    {
        // Arrange
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create();
        _mockApiHttpClient.GetAsync<ScheduledJobExecutionDto[]>(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ScheduledJobExecutionDto[]>(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyFromIsProvided_ShouldRequestHistoryWithFromQueryString()
    {
        // Arrange
        DateTime from = new(2026, 1, 5, 4, 3, 2, DateTimeKind.Utc);
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create(from: from);
        string expectedEndpoint = $"{ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY}?from={Uri.EscapeDataString(from.ToString("o"))}";
        _mockApiHttpClient.GetAsync<ScheduledJobExecutionDto[]>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ScheduledJobExecutionDto[]>(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyToIsProvided_ShouldRequestHistoryWithToQueryString()
    {
        // Arrange
        DateTime to = new(2026, 1, 5, 4, 3, 2, DateTimeKind.Utc);
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create(to: to);
        string expectedEndpoint = $"{ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY}?to={Uri.EscapeDataString(to.ToString("o"))}";
        _mockApiHttpClient.GetAsync<ScheduledJobExecutionDto[]>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ScheduledJobExecutionDto[]>(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFromAndToAreProvided_ShouldRequestHistoryWithBothQueryStrings()
    {
        // Arrange
        DateTime from = new(2026, 1, 5, 4, 3, 2, DateTimeKind.Utc);
        DateTime to = new(2026, 1, 6, 4, 3, 2, DateTimeKind.Utc);
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create(from: from, to: to);
        string expectedEndpoint = $"{ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY}?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
        _mockApiHttpClient.GetAsync<ScheduledJobExecutionDto[]>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ScheduledJobExecutionDto[]>(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
