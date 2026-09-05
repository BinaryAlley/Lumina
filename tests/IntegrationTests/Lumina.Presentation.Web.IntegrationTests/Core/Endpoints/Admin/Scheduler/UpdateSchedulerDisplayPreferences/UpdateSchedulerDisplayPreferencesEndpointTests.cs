#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.UpdateSchedulerDisplayPreferences;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/api-scheduled-jobs/display-preferences</c> route served by the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdateSchedulerDisplayPreferencesRequestFixture _updateSchedulerDisplayPreferencesRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateSchedulerDisplayPreferencesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldUpdateDisplayPreferencesAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPutResponseFactory("scheduled-jobs/display-preferences", _ => new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/admin/api-scheduled-jobs/display-preferences")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "scheduled-jobs/display-preferences");
    }
}
