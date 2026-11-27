#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StopConsumption;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.MediaConsumption.StopConsumption;

/// <summary>
/// Contains integration tests for the <see cref="StopConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly StopConsumptionRequestFixture _stopConsumptionRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StopConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StopConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StopConsumption_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldStopThroughApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/stop";
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(
            status: ConsumptionStatus.Stopped,
            lastProgressFraction: 0.3,
            consecutivePagesRead: 8);
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.Location),
            includeTimeOffsetSeconds: false);
        StopConsumptionRequest stopRequest = _stopConsumptionRequestFixture.Create(
            position: position,
            progressFraction: 0.3,
            consumedDurationInSeconds: 95,
            consecutivePagesRead: 8,
            includeConsecutivePagesRead: true);
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStopRequest(s_recordId.ToString(), stopRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedRecord.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(nameof(ConsumptionStatus.Stopped), data.GetProperty("status").GetString());
        Assert.Equal(0.3, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(8, data.GetProperty("consecutivePagesRead").GetInt32());
        (string Endpoint, object? Data) capturedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.Equal(stopRequest, Assert.IsType<StopConsumptionRequest>(capturedRequest.Data));
        Assert.DoesNotContain(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint != ApiRoutes.Authentication.LOGIN_ACCOUNT && postRequest.Endpoint != expectedEndpoint);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task StopConsumption_WhenRecordIdIsNotAGuid_ShouldUseTheEmptyGuidInTheUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/consumption-records/{ZERO_GUID}/stop";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        StopConsumptionRequest stopRequest = _stopConsumptionRequestFixture.Create();
        HttpRequestMessage request = CreateStopRequest("not-a-guid", stopRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == sanitizedEndpoint);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.Contains("not-a-guid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StopConsumption_WhenRecordIdIsUppercase_ShouldNormalizeItToLowercaseInTheUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid recordId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string uppercaseRecordId = recordId.ToString().ToUpperInvariant();
        string expectedEndpoint = $"users/me/consumption-records/{recordId}/stop";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        StopConsumptionRequest stopRequest = _stopConsumptionRequestFixture.Create();
        HttpRequestMessage request = CreateStopRequest(uppercaseRecordId, stopRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.Contains(uppercaseRecordId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task StopConsumption_WhenPositionIsOmitted_ShouldForwardTheRequestWithoutAPosition()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/stop";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        StopConsumptionRequest stopRequest = _stopConsumptionRequestFixture.Create(
            progressFraction: 0.55,
            consumedDurationInSeconds: 210,
            consecutivePagesRead: 6,
            includePosition: false);
        HttpRequestMessage request = CreateStopRequest(s_recordId.ToString(), stopRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) capturedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.Equal(expectedEndpoint, capturedRequest.Endpoint);
        StopConsumptionRequest forwardedRequest = Assert.IsType<StopConsumptionRequest>(capturedRequest.Data);
        Assert.Null(forwardedRequest.Position);
        Assert.Equal(0.55, forwardedRequest.ProgressFraction!.Value);
        Assert.Equal(210, forwardedRequest.ConsumedDurationInSeconds!.Value);
        Assert.Equal(6, forwardedRequest.ConsecutivePagesRead!.Value);
    }

    /// <summary>
    /// Builds the stop consumption request for the specified record.
    /// </summary>
    /// <param name="recordId">The value of the record identifier route segment.</param>
    /// <param name="stopRequest">The stop payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured stop consumption request.</returns>
    private static HttpRequestMessage CreateStopRequest(string recordId, StopConsumptionRequest stopRequest, string antiforgeryToken)
    {
        HttpRequestMessage stoppingRequest = new(HttpMethod.Post, $"/en-us/media-consumption/api-stop-consumption/{recordId}")
        {
            Content = JsonContent.Create(stopRequest)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        stoppingRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        stoppingRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        stoppingRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return stoppingRequest;
    }
}
