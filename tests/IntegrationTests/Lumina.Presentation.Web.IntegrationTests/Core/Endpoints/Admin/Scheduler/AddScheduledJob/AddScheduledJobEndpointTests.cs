#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.AddScheduledJob;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Scheduler.AddScheduledJob;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/api-scheduled-jobs/add</c> route served by the <see cref="AddScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly AddScheduledJobRequestFixture _addScheduledJobRequestFixture = new();
    private readonly ScheduledJobDtoFixture _scheduledJobDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddScheduledJobEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldAddScheduledJobAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostResponse("scheduled-jobs", _scheduledJobDtoFixture.Create(name: request.Name));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = new(HttpMethod.Post, "/en-us/admin/api-scheduled-jobs/add")
        {
            Content = JsonContent.Create(request)
        };
        addRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        addRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        addRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "scheduled-jobs");
    }
}
