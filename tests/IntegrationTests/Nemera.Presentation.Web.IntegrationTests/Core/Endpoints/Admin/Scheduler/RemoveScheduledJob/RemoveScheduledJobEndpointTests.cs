#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.RemoveScheduledJob;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.RemoveScheduledJob;

/// <summary>
/// Contains integration tests for the <see cref="RemoveScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly RemoveScheduledJobRequestFixture _removeScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RemoveScheduledJobEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RemoveScheduledJob_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldRemoveScheduledJobAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        RemoveScheduledJobRequest request = _removeScheduledJobRequestFixture.Create();
        string expectedEndpoint = $"scheduled-jobs/{request.ScheduledJobId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = new(HttpMethod.Delete, $"/en-us/admin/api-scheduled-jobs/{request.ScheduledJobId}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        removeRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        removeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        removeRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint == expectedEndpoint);
    }
}
