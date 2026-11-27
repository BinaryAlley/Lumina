#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetMostConsumed;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Statistics.GetMostConsumed;

/// <summary>
/// Contains integration tests for the <see cref="GetMostConsumedEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private static readonly string s_userId = "11111111-1111-1111-1111-111111111111";
    private static readonly string s_libraryId = "22222222-2222-2222-2222-222222222222";
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10);
    private static readonly Guid s_mediaItemId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetMostConsumedRequestFixture _getMostConsumedRequestFixture = new();
    private readonly ConsumedItemDtoFixture _consumedItemDtoFixture = new();
    private readonly PaginatedDtoFixture<ConsumedItemDto> _paginatedDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetMostConsumedEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetMostConsumed_WhenUserIdFiltersAndPagingAreProvided_ShouldReturnPageJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string EXPECTED_UPSTREAM_PATH = "users/11111111-1111-1111-1111-111111111111/consumption-history/most-consumed?libraryId=22222222-2222-2222-2222-222222222222&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000&toUtc=2025-06-07T08%3A09%3A10.0000000&currentPage=2&perPage=5";
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            userId: s_userId,
            libraryId: s_libraryId,
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc,
            currentPage: 2,
            perPage: 5);
        PaginatedDto<ConsumedItemDto> expectedPage = _paginatedDtoFixture.Create(
            data:
            [
                _consumedItemDtoFixture.Create(mediaItemId: s_mediaItemId, mediaKind: MediaKind.Movie),
                _consumedItemDtoFixture.Create(mediaKind: MediaKind.Movie)
            ],
            currentPage: 2,
            perPage: 5,
            count: 12,
            numberOfPages: 3);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedPage);
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
        Assert.Equal(2, data.GetProperty("data").GetArrayLength());
        Assert.Equal(2, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(5, data.GetProperty("perPage").GetInt32());
        Assert.Equal(s_mediaItemId, data.GetProperty("data")[0].GetProperty("mediaItemId").GetGuid());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMostConsumed_WhenNoFiltersOrPagingAreProvided_ShouldRequestCurrentUserWithDefaultPaging()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string EXPECTED_UPSTREAM_PATH = "users/me/consumption-history/most-consumed?currentPage=1&perPage=10";
        GetMostConsumedRequest request = _getMostConsumedRequestFixture.Create(
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false,
            includeCurrentPage: false,
            includePerPage: false);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, _paginatedDtoFixture.Create(data: [], currentPage: 1, perPage: 10));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(0, data.GetProperty("data").GetArrayLength());
        Assert.Equal(1, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(10, data.GetProperty("perPage").GetInt32());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
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
    /// <param name="request">The request whose filters and paging are forwarded.</param>
    /// <returns>The path of the Web route, including the query string built from the request values.</returns>
    private static string BuildRequestPath(GetMostConsumedRequest request)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.UserId))
            parameters.Add($"userId={Uri.EscapeDataString(request.UserId)}");
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            parameters.Add($"libraryId={Uri.EscapeDataString(request.LibraryId)}");
        if (request.MediaKind is not null)
            parameters.Add($"mediaKind={Uri.EscapeDataString(request.MediaKind.Value.ToString())}");
        if (request.FromUtc is not null)
            parameters.Add($"fromUtc={Uri.EscapeDataString(request.FromUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (request.ToUtc is not null)
            parameters.Add($"toUtc={Uri.EscapeDataString(request.ToUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (request.CurrentPage is not null)
            parameters.Add($"currentPage={request.CurrentPage.Value.ToString(CultureInfo.InvariantCulture)}");
        if (request.PerPage is not null)
            parameters.Add($"perPage={request.PerPage.Value.ToString(CultureInfo.InvariantCulture)}");
        return parameters.Count == 0
            ? "/en-us/library/statistics/api-get-most-consumed"
            : $"/en-us/library/statistics/api-get-most-consumed?{string.Join("&", parameters)}";
    }
}
