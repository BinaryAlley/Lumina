#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Scheduling;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobs;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.GetScheduledJobs;

/// <summary>
/// Contains integration tests for the <see cref="GetScheduledJobsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobsEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly ScheduledJobDtoFixture _scheduledJobDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetScheduledJobsEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetScheduledJobs_WhenCalledByAuthenticatedAdmin_ShouldReturnScheduledJobsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ScheduledJobDto[] expectedJobs = [.. _scheduledJobDtoFixture.CreateMany(2)];
        _apiFactory.ApiClientStub.RegisterGetResponse("scheduled-jobs", expectedJobs);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/api-scheduled-jobs");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedJobs.Length, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "scheduled-jobs");
    }
}
