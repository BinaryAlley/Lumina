#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.FireScheduledJob;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.FireScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="FireScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly FireScheduledJobEndpoint _sut;
    private readonly FireScheduledJobRequestFixture _fireScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobEndpointTests"/> class.
    /// </summary>
    public FireScheduledJobEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<FireScheduledJobEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldFireScheduledJobViaApiAndReturnSuccess()
    {
        // Arrange
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.ScheduledJobs.FIRE_SCHEDULED_JOB.Replace("{scheduledJobId}", request.ScheduledJobId.ToString());
        await _mockApiHttpClient.Received(1).PutAsync<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest, Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(
            expectedEndpoint,
            Arg.Any<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
