#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// Contains integration tests for the <see cref="ReportConsumptionProgressEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly ReportConsumptionProgressRequestFixture _reportConsumptionProgressRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ReportConsumptionProgressEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldReportThroughApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/progress";
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(
            status: ConsumptionStatus.InProgress,
            lastProgressFraction: 0.42,
            consecutivePagesRead: 17);
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.Location),
            includeTimeOffsetSeconds: false);
        ReportConsumptionProgressRequest reportRequest = _reportConsumptionProgressRequestFixture.Create(
            position: position,
            progressFraction: 0.42,
            consumedDurationInSeconds: 120,
            consecutivePagesRead: 17);
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), reportRequest, webClient.AntiforgeryToken);

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
        Assert.Equal(nameof(ConsumptionStatus.InProgress), data.GetProperty("status").GetString());
        Assert.Equal(0.42, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(17, data.GetProperty("consecutivePagesRead").GetInt32());
        (string Endpoint, object? Data) capturedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PutRequests,
            putRequest => putRequest.Endpoint == expectedEndpoint);
        Assert.Equal(expectedEndpoint, capturedRequest.Endpoint);
        Assert.Equal(reportRequest, Assert.IsType<ReportConsumptionProgressRequest>(capturedRequest.Data));
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenRecordIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{ZERO_GUID}/progress";
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ReportConsumptionProgressRequest reportRequest = _reportConsumptionProgressRequestFixture.Create();
        HttpRequestMessage request = CreateReportRequest("not-a-guid", reportRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint.Contains("not-a-guid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenRecordIdIsUppercase_ShouldNormalizeItToLowercaseInTheUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid recordId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string uppercaseRecordId = recordId.ToString().ToUpperInvariant();
        string expectedEndpoint = $"users/me/consumption-records/{recordId}/progress";
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ReportConsumptionProgressRequest reportRequest = _reportConsumptionProgressRequestFixture.Create();
        HttpRequestMessage request = CreateReportRequest(uppercaseRecordId, reportRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint.Contains(uppercaseRecordId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenPositionIsOmitted_ShouldForwardTheRequestWithoutAPosition()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/progress";
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ReportConsumptionProgressRequest reportRequest = _reportConsumptionProgressRequestFixture.Create(
            progressFraction: 0.6,
            consumedDurationInSeconds: 180,
            consecutivePagesRead: 9,
            includePosition: false);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), reportRequest, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) capturedRequest = Assert.Single(
            _apiFactory.ApiClientStub.PutRequests,
            putRequest => putRequest.Endpoint == expectedEndpoint);
        Assert.Equal(expectedEndpoint, capturedRequest.Endpoint);
        ReportConsumptionProgressRequest forwardedRequest = Assert.IsType<ReportConsumptionProgressRequest>(capturedRequest.Data);
        Assert.Null(forwardedRequest.Position);
        Assert.Equal(0.6, forwardedRequest.ProgressFraction!.Value);
        Assert.Equal(180, forwardedRequest.ConsumedDurationInSeconds!.Value);
        Assert.Equal(9, forwardedRequest.ConsecutivePagesRead);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenCalledWithPost_ShouldRejectTheRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ReportConsumptionProgressRequest reportRequest = _reportConsumptionProgressRequestFixture.Create();
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), reportRequest, webClient.AntiforgeryToken, HttpMethod.Post);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the report consumption progress request for the specified record.
    /// </summary>
    /// <param name="recordId">The value of the record identifier route segment.</param>
    /// <param name="reportRequest">The progress payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <param name="method">The HTTP method of the request, which defaults to the <c>PUT</c> the route requires.</param>
    /// <returns>The configured report consumption progress request.</returns>
    private static HttpRequestMessage CreateReportRequest(string recordId, ReportConsumptionProgressRequest reportRequest, string antiforgeryToken, HttpMethod? method = null)
    {
        HttpRequestMessage progressRequest = new(method ?? HttpMethod.Put, $"/en-us/media-consumption/api-report-consumption-progress/{recordId}")
        {
            Content = JsonContent.Create(reportRequest)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        progressRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        progressRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        progressRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return progressRequest;
    }
}
