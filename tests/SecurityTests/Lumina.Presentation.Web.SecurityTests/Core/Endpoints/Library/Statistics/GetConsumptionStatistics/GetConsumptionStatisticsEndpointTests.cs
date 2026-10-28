#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// Contains security tests for the <see cref="GetConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/library/statistics/api-get-statistics";

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetConsumptionStatisticsRequestFixture _getConsumptionStatisticsRequestFixture = new();
    private readonly ConsumptionSummaryDtoFixture _consumptionSummaryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetConsumptionStatisticsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetConsumptionStatistics_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, WEB_ROUTE);
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData("Movie%2F..%2Fadmin%3Fx%3D1")] // path traversal attempt in the media kind
    [InlineData("NotAMediaKind")] // undefined enum member
    public async Task GetConsumptionStatistics_WithCraftedMediaKind_ShouldRejectRequestAndNotCallApi(string craftedMediaKind)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"{WEB_ROUTE}?mediaKind={craftedMediaKind}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("users/", StringComparison.Ordinal));
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetConsumptionStatistics_WithCraftedUserAndLibraryIds_ShouldEscapeTheValuesAndKeepTheDocumentedUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string CRAFTED_USER_ID = "user/../admin?x=1&y=2";
        const string CRAFTED_LIBRARY_ID = "lib?x=1&y=2";
        const string EXPECTED_UPSTREAM_PATH = "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-statistics?libraryId=lib%3Fx%3D1%26y%3D2";
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            userId: CRAFTED_USER_ID,
            libraryId: CRAFTED_LIBRARY_ID,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, _consumptionSummaryDtoFixture.Create(includeMediaKindSummaries: false));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        string[] dataEndpoints = [.. _apiFactory.ApiClientStub.GetEndpointsCalled.Where(endpoint => endpoint.StartsWith("users/", StringComparison.Ordinal))];
        Assert.Equal([EXPECTED_UPSTREAM_PATH], dataEndpoints);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Builds the path of the Web route for <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request whose filters are forwarded.</param>
    /// <returns>The path of the Web route, including the query string built from the request values.</returns>
    private static string BuildRequestPath(GetConsumptionStatisticsRequest request)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.UserId))
            parameters.Add($"userId={Uri.EscapeDataString(request.UserId)}");
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            parameters.Add($"libraryId={Uri.EscapeDataString(request.LibraryId)}");
        return parameters.Count == 0
            ? WEB_ROUTE
            : $"{WEB_ROUTE}?{string.Join("&", parameters)}";
    }
}
