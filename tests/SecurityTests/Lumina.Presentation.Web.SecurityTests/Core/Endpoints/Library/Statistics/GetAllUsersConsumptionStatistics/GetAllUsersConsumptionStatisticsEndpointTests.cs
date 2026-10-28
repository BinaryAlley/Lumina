#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains security tests for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/library/statistics/api-get-all-users-statistics";

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAllUsersConsumptionStatisticsRequestFixture _getAllUsersConsumptionStatisticsRequestFixture = new();
    private readonly AllUsersConsumptionStatisticsDtoFixture _allUsersConsumptionStatisticsDtoFixture = new();
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetAllUsersConsumptionStatisticsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
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

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByNonAdminUser_ShouldRedirectToAccessDeniedAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("users/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByAdminUser_ShouldReturnStatisticsWithoutLeakingSecrets()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        const string EXPECTED_UPSTREAM_PATH = "users/consumption-statistics";
        AllUsersConsumptionStatisticsDto expectedStatistics = _allUsersConsumptionStatisticsDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedStatistics);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WithCraftedLibraryId_ShouldEscapeTheValueAndKeepTheDocumentedUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        const string CRAFTED_LIBRARY_ID = "lib?x=1&y=2";
        const string EXPECTED_UPSTREAM_PATH = "users/consumption-statistics?libraryId=lib%3Fx%3D1%26y%3D2";
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: CRAFTED_LIBRARY_ID,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, _allUsersConsumptionStatisticsDtoFixture.Create(includeUsers: false));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string[] dataEndpoints = [.. _apiFactory.ApiClientStub.GetEndpointsCalled.Where(endpoint => endpoint.StartsWith("users/", StringComparison.Ordinal))];
        Assert.Equal([EXPECTED_UPSTREAM_PATH], dataEndpoints);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
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
    /// <returns>The path of the Web route, including the query string built from the request filters.</returns>
    private static string BuildRequestPath(GetAllUsersConsumptionStatisticsRequest request)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            parameters.Add($"libraryId={Uri.EscapeDataString(request.LibraryId)}");
        return parameters.Count == 0
            ? WEB_ROUTE
            : $"{WEB_ROUTE}?{string.Join("&", parameters)}";
    }
}
