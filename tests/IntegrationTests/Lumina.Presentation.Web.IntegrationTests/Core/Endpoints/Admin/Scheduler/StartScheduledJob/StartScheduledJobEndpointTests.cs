#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.StartScheduledJob;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.StartScheduledJob;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/api-scheduled-jobs/{scheduledJobId}/start</c> route served by the <see cref="StartScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly StartScheduledJobRequestFixture _startScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StartScheduledJobEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StartScheduledJob_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldStartScheduledJobAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create();
        string expectedEndpoint = $"scheduled-jobs/{request.ScheduledJobId}/start";
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage startRequest = new(HttpMethod.Put, $"/en-us/admin/api-scheduled-jobs/{request.ScheduledJobId}/start")
        {
            Content = JsonContent.Create(new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest())
        };
        startRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        startRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        startRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(startRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
    }
}
