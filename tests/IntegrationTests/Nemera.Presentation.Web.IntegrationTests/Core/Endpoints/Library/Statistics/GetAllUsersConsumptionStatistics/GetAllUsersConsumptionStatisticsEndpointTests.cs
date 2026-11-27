#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains integration tests for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private static readonly string s_libraryId = "22222222-2222-2222-2222-222222222222";
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10);

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAllUsersConsumptionStatisticsRequestFixture _getAllUsersConsumptionStatisticsRequestFixture = new();
    private readonly AllUsersConsumptionStatisticsDtoFixture _allUsersConsumptionStatisticsDtoFixture = new();
    private readonly ConsumptionTotalsDtoFixture _consumptionTotalsDtoFixture = new();
    private readonly ConsumptionSummaryDtoFixture _consumptionSummaryDtoFixture = new();
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
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByAdminWithAllFilters_ShouldReturnStatisticsJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: s_libraryId,
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc);
        const string EXPECTED_UPSTREAM_PATH = "users/consumption-statistics?libraryId=22222222-2222-2222-2222-222222222222&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000&toUtc=2025-06-07T08%3A09%3A10.0000000";
        ConsumptionTotalsDto total = _consumptionTotalsDtoFixture.Create(countedSessionCount: 42, includeMediaKindSummaries: false);
        ConsumptionSummaryDto userSummary = _consumptionSummaryDtoFixture.Create(countedSessionCount: 7, includeMediaKindSummaries: false);
        AllUsersConsumptionStatisticsDto expectedStatistics = _allUsersConsumptionStatisticsDtoFixture.Create(total: total, users: [userSummary]);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedStatistics);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(42, data.GetProperty("total").GetProperty("countedSessionCount").GetInt32());
        Assert.Equal(1, data.GetProperty("users").GetArrayLength());
        Assert.Equal(7, data.GetProperty("users")[0].GetProperty("countedSessionCount").GetInt32());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByAdminWithoutFilters_ShouldRequestEveryUserAndHandleEmptyUsers()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        const string EXPECTED_UPSTREAM_PATH = "users/consumption-statistics";
        AllUsersConsumptionStatisticsDto expectedStatistics = _allUsersConsumptionStatisticsDtoFixture.Create(includeUsers: false);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedStatistics);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.Equal(0, json.RootElement.GetProperty("data").GetProperty("users").GetArrayLength());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetAllUsersConsumptionStatistics_WhenCalledByNonAdminUser_ShouldRedirectToAccessDeniedAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("users/", StringComparison.Ordinal));
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
        if (request.MediaKind is not null)
            parameters.Add($"mediaKind={Uri.EscapeDataString(request.MediaKind.Value.ToString())}");
        if (request.FromUtc is not null)
            parameters.Add($"fromUtc={Uri.EscapeDataString(request.FromUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (request.ToUtc is not null)
            parameters.Add($"toUtc={Uri.EscapeDataString(request.ToUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        return parameters.Count == 0
            ? "/en-us/library/statistics/api-get-all-users-statistics"
            : $"/en-us/library/statistics/api-get-all-users-statistics?{string.Join("&", parameters)}";
    }
}
