#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.StartScheduledJob;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.StartScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StartScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly StartScheduledJobEndpoint _sut;
    private readonly StartScheduledJobRequestFixture _startScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpointTests"/> class.
    /// </summary>
    public StartScheduledJobEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<StartScheduledJobEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldStartScheduledJobViaApiAndReturnSuccess()
    {
        // Arrange
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.ScheduledJobs.START_SCHEDULED_JOB.Replace("{scheduledJobId}", request.ScheduledJobId.ToString());
        await _mockApiHttpClient.Received(1).PutAsync<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest, Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(
            expectedEndpoint,
            Arg.Any<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
