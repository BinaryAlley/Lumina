#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.FireScheduledJob;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.FireScheduledJob;

/// <summary>
/// Contains integration tests for the <see cref="FireScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly FireScheduledJobRequestFixture _fireScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public FireScheduledJobEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task FireScheduledJob_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldFireScheduledJobAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();
        string expectedEndpoint = $"scheduled-jobs/{request.ScheduledJobId}/fire";
        _apiFactory.ApiClientStub.RegisterPutPayload<FireScheduledJobRequest>(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage fireRequest = new(HttpMethod.Put, $"/en-us/admin/api-scheduled-jobs/{request.ScheduledJobId}/fire")
        {
            Content = JsonContent.Create(request)
        };
        fireRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        fireRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        fireRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(fireRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
    }
}
