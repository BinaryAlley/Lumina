#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Scheduling;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetSchedulerDisplayPreferences;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Scheduling;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.GetSchedulerDisplayPreferences;

/// <summary>
/// Contains integration tests for the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly SchedulerDisplayPreferencesDtoFixture _schedulerDisplayPreferencesDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetSchedulerDisplayPreferencesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetSchedulerDisplayPreferences_WhenCalledByAuthenticatedAdmin_ShouldReturnDisplayPreferencesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        SchedulerDisplayPreferencesDto expectedPreferences = _schedulerDisplayPreferencesDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse("scheduled-jobs/display-preferences", expectedPreferences);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/api-scheduled-jobs/display-preferences");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPreferences.UserId, json.RootElement.GetProperty("data").GetProperty("userId").GetGuid());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "scheduled-jobs/display-preferences");
    }
}
