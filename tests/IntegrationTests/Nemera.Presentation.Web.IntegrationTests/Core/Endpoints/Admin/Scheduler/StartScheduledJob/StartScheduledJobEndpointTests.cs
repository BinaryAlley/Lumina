#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.StartScheduledJob;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.StartScheduledJob;

/// <summary>
/// Contains integration tests for the <see cref="StartScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly StartScheduledJobRequestFixture _startScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StartScheduledJobEndpointTests(NemeraWebFactory apiFactory)
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
        _apiFactory.ApiClientStub.RegisterPutPayload<StartScheduledJobRequest>(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage startRequest = new(HttpMethod.Put, $"/en-us/admin/api-scheduled-jobs/{request.ScheduledJobId}/start")
        {
            Content = JsonContent.Create(request)
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
