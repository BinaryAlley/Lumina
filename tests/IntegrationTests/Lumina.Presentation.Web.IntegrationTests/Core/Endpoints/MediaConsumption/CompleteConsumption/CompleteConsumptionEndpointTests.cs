#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.CompleteConsumption;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.MediaConsumption.CompleteConsumption;

/// <summary>
/// Contains integration tests for the <see cref="CompleteConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly CompleteConsumptionRequestFixture _completeConsumptionRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CompleteConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CompleteConsumption_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldCompleteThroughApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/complete";
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(
            status: ConsumptionStatus.Completed,
            lastProgressFraction: 1,
            consecutivePagesRead: 12);
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.TimeOffset),
            includeLocation: false);
        CompleteConsumptionRequest completeRequest = _completeConsumptionRequestFixture.Create(
            position: position,
            progressFraction: 1,
            consumedDurationInSeconds: 240,
            consecutivePagesRead: 12,
            includeConsecutivePagesRead: true);
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), completeRequest, webClient.AntiforgeryToken);

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
        Assert.Equal(nameof(ConsumptionStatus.Completed), data.GetProperty("status").GetString());
        Assert.Equal(1, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(12, data.GetProperty("consecutivePagesRead").GetInt32());
        (string Endpoint, object? Data) capturedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.Equal(completeRequest, Assert.IsType<CompleteConsumptionRequest>(capturedRequest.Data));
        Assert.DoesNotContain(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint != ApiRoutes.Authentication.LOGIN_ACCOUNT && postRequest.Endpoint != expectedEndpoint);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task CompleteConsumption_WhenRecordIdIsNotAGuid_ShouldUseTheEmptyGuidInTheUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/consumption-records/{ZERO_GUID}/complete";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        CompleteConsumptionRequest completeRequest = _completeConsumptionRequestFixture.Create();
        HttpRequestMessage request = CreateCompleteRequest("not-a-guid", completeRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == sanitizedEndpoint);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.Contains("not-a-guid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CompleteConsumption_WhenRecordIdIsUppercase_ShouldNormalizeItToLowercaseInTheUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid recordId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string uppercaseRecordId = recordId.ToString().ToUpperInvariant();
        string expectedEndpoint = $"users/me/consumption-records/{recordId}/complete";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        CompleteConsumptionRequest completeRequest = _completeConsumptionRequestFixture.Create();
        HttpRequestMessage request = CreateCompleteRequest(uppercaseRecordId, completeRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.Contains(uppercaseRecordId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task CompleteConsumption_WhenPositionIsOmitted_ShouldForwardTheRequestWithoutAPosition()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/complete";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        CompleteConsumptionRequest completeRequest = _completeConsumptionRequestFixture.Create(
            progressFraction: 0.75,
            consumedDurationInSeconds: 300,
            consecutivePagesRead: 4,
            includePosition: false);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), completeRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) capturedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PostRequests,
            postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.Equal(expectedEndpoint, capturedRequest.Endpoint);
        CompleteConsumptionRequest forwardedRequest = Assert.IsType<CompleteConsumptionRequest>(capturedRequest.Data);
        Assert.Null(forwardedRequest.Position);
        Assert.Equal(0.75, forwardedRequest.ProgressFraction!.Value);
        Assert.Equal(300, forwardedRequest.ConsumedDurationInSeconds!.Value);
        Assert.Equal(4, forwardedRequest.ConsecutivePagesRead!.Value);
    }

    /// <summary>
    /// Builds the complete consumption request for the specified record.
    /// </summary>
    /// <param name="recordId">The value of the record identifier route segment.</param>
    /// <param name="completeRequest">The completion payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured complete consumption request.</returns>
    private static HttpRequestMessage CreateCompleteRequest(string recordId, CompleteConsumptionRequest completeRequest, string antiforgeryToken)
    {
        HttpRequestMessage completionRequest = new(HttpMethod.Post, $"/en-us/media-consumption/api-complete-consumption/{recordId}")
        {
            Content = JsonContent.Create(completeRequest)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        completionRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        completionRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        completionRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return completionRequest;
    }
}
