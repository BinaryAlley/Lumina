#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobHistory;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.GetScheduledJobHistory;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/api-scheduled-jobs/history</c> route served by the <see cref="GetScheduledJobHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ScheduledJobExecutionDtoFixture _scheduledJobExecutionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetScheduledJobHistoryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetScheduledJobHistory_WhenCalledByAuthenticatedAdmin_ShouldReturnExecutionHistoryFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ScheduledJobExecutionDto[] expectedExecutions = [.. _scheduledJobExecutionDtoFixture.CreateMany(2)];
        _apiFactory.ApiClientStub.RegisterGetResponse("scheduled-jobs/history", expectedExecutions);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/api-scheduled-jobs/history");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedExecutions.Length, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "scheduled-jobs/history");
    }
}
