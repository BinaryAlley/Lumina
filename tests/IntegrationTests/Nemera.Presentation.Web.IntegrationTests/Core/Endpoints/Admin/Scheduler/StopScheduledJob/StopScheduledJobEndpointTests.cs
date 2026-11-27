#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.StopScheduledJob;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.StopScheduledJob;

/// <summary>
/// Contains integration tests for the <see cref="StopScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly StopScheduledJobRequestFixture _stopScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StopScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StopScheduledJobEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StopScheduledJob_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldStopScheduledJobAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        StopScheduledJobRequest request = _stopScheduledJobRequestFixture.Create();
        string expectedEndpoint = $"scheduled-jobs/{request.ScheduledJobId}/stop";
        _apiFactory.ApiClientStub.RegisterPutPayload<StopScheduledJobRequest>(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage stopRequest = new(HttpMethod.Put, $"/en-us/admin/api-scheduled-jobs/{request.ScheduledJobId}/stop")
        {
            Content = JsonContent.Create(request)
        };
        stopRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        stopRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        stopRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(stopRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
    }
}
