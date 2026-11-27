#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.StopScheduledJob;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.StopScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StopScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly StopScheduledJobEndpoint _sut;
    private readonly StopScheduledJobRequestFixture _stopScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StopScheduledJobEndpointTests"/> class.
    /// </summary>
    public StopScheduledJobEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<StopScheduledJobEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldStopScheduledJobViaApiAndReturnSuccess()
    {
        // Arrange
        StopScheduledJobRequest request = _stopScheduledJobRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.ScheduledJobs.STOP_SCHEDULED_JOB.Replace("{scheduledJobId}", request.ScheduledJobId.ToString());
        await _mockApiHttpClient.Received(1).PutIgnoringResponseAsync<StopScheduledJobRequest>(
            expectedEndpoint,
            Arg.Is<StopScheduledJobRequest>(forwardedRequest => forwardedRequest.ScheduledJobId == request.ScheduledJobId),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
