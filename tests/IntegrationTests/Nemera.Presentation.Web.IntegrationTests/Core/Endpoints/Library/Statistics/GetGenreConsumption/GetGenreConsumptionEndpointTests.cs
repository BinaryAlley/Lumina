#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetGenreConsumption;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// Contains integration tests for the <see cref="GetGenreConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private static readonly string s_userId = "11111111-1111-1111-1111-111111111111";
    private static readonly string s_libraryId = "22222222-2222-2222-2222-222222222222";
    private static readonly DateTime s_fromUtc = new(2024, 3, 4, 5, 6, 7);
    private static readonly DateTime s_toUtc = new(2025, 6, 7, 8, 9, 10);

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetGenreConsumptionRequestFixture _getGenreConsumptionRequestFixture = new();
    private readonly GenreConsumptionDtoFixture _genreConsumptionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetGenreConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetGenreConsumption_WhenUserIdAndAllFiltersAreProvided_ShouldReturnGenreConsumptionJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string EXPECTED_UPSTREAM_PATH = "users/11111111-1111-1111-1111-111111111111/consumption-statistics/genres?libraryId=22222222-2222-2222-2222-222222222222&mediaKind=Movie&fromUtc=2024-03-04T05%3A06%3A07.0000000&toUtc=2025-06-07T08%3A09%3A10.0000000";
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            userId: s_userId,
            libraryId: s_libraryId,
            mediaKind: MediaKind.Movie,
            fromUtc: s_fromUtc,
            toUtc: s_toUtc);
        GenreConsumptionDto[] expectedGenres = [_genreConsumptionDtoFixture.Create(name: "drama", consumedItemCount: 3)];
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedGenres);
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
        Assert.Equal(1, data.GetArrayLength());
        Assert.Equal("drama", data[0].GetProperty("name").GetString());
        Assert.Equal(3, data[0].GetProperty("consumedItemCount").GetInt32());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGenreConsumption_WhenCalledWithoutFilters_ShouldRequestCurrentUserGenresAndHandleEmptyResult()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string EXPECTED_UPSTREAM_PATH = "users/me/consumption-statistics/genres";
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            includeUserId: false,
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, Array.Empty<GenreConsumptionDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.Equal(0, json.RootElement.GetProperty("data").GetArrayLength());
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
    /// <param name="request">The request whose filters are forwarded.</param>
    /// <returns>The path of the Web route, including the query string built from the request filters.</returns>
    private static string BuildRequestPath(GetGenreConsumptionRequest request)
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
        return parameters.Count == 0
            ? "/en-us/library/statistics/api-get-genre-consumption"
            : $"/en-us/library/statistics/api-get-genre-consumption?{string.Join("&", parameters)}";
    }
}
