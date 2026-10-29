#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// Contains security tests for the <see cref="ReportConsumptionProgressEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
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
    public async Task ReportConsumptionProgress_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), _reportConsumptionProgressRequestFixture.Create(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), _reportConsumptionProgressRequestFixture.Create(), null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), _reportConsumptionProgressRequestFixture.Create(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenCalledByAuthenticatedUserWithValidToken_ShouldCallApiOnceWithoutLeakingSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/progress";
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(status: ConsumptionStatus.InProgress);
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), _reportConsumptionProgressRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        (string Endpoint, object? Data) capturedRequest = Assert.Single(_apiFactory.ApiClientStub.PutRequests);
        Assert.Equal(expectedEndpoint, capturedRequest.Endpoint);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task ReportConsumptionProgress_WithInjectionPayloadInRecordId_ShouldUseTheEmptyGuidAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/consumption-records/{ZERO_GUID}/progress";
        _apiFactory.ApiClientStub.RegisterPutResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateReportRequest(Uri.EscapeDataString(payload), _reportConsumptionProgressRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) capturedRequest = Assert.Single(_apiFactory.ApiClientStub.PutRequests);
        Assert.Equal(sanitizedEndpoint, capturedRequest.Endpoint);
        Assert.DoesNotContain(payload, capturedRequest.Endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), capturedRequest.Endpoint, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReportConsumptionProgress_WhenApiFails_ShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/progress";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateReportRequest(s_recordId.ToString(), _reportConsumptionProgressRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Builds the report consumption progress request for the specified record.
    /// </summary>
    /// <param name="recordId">The value of the record identifier route segment.</param>
    /// <param name="reportRequest">The progress payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured report consumption progress request.</returns>
    private static HttpRequestMessage CreateReportRequest(string recordId, ReportConsumptionProgressRequest reportRequest, string? antiforgeryToken)
    {
        HttpRequestMessage progressRequest = new(HttpMethod.Put, $"/en-us/media-consumption/api-report-consumption-progress/{recordId}")
        {
            Content = JsonContent.Create(reportRequest)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        progressRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        progressRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            progressRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return progressRequest;
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }
}
