#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Enums.Common;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;

/// <summary>
/// Contains unit tests for the <see cref="GetMusicArtistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetMusicArtistsEndpoint _sut;
    private readonly GetMusicArtistsLiteRequestFixture _getMusicArtistsLiteRequestFixture = new();
    private readonly PaginatedMusicArtistLiteDtoFixture _paginatedMusicArtistLiteDtoFixture = new();
    private readonly MusicArtistLiteDtoFixture _musicArtistLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistsEndpointTests"/> class.
    /// </summary>
    public GetMusicArtistsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetMusicArtistsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithAllParameters_ShouldBuildExactQueryString()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        const string SEARCH_TERM = "  The Beatles  ";
        const string FILTER_ALPHA_KEY = "B";
        const string SORT_BY = "name asc";
        GetMusicArtistsLiteRequest request = _getMusicArtistsLiteRequestFixture.Create(
            libraryId: libraryId,
            currentPage: 2,
            perPage: 48,
            searchTerm: SEARCH_TERM,
            filterAlphaKey: FILTER_ALPHA_KEY,
            shouldIgnoreThePrefixForAlphaPicker: true,
            sortBy: SORT_BY,
            sortOrder: SortOrder.Descending);
        _mockApiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicArtistLiteDtoFixture.Create());
        string expectedEndpoint = $"{BuildExpectedEndpoint(libraryId)}?currentPage=2&perPage=48" +
            $"&searchTerm={Uri.EscapeDataString(SEARCH_TERM)}" +
            $"&filterAlphaKey={Uri.EscapeDataString(FILTER_ALPHA_KEY)}" +
            "&shouldIgnoreThePrefixForAlphaPicker=True" +
            $"&sortBy={Uri.EscapeDataString(SORT_BY)}" +
            "&sortOrder=Descending";

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicArtistLiteDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionalParametersAreOmitted_ShouldAppendOnlyTheIgnorePrefixFlag()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetMusicArtistsLiteRequest request = _getMusicArtistsLiteRequestFixture.Create(
            libraryId: libraryId,
            includeCurrentPage: false,
            includePerPage: false,
            includeSearchTerm: false,
            includeFilterAlphaKey: false,
            includeSortBy: false,
            includeSortOrder: false);
        _mockApiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicArtistLiteDtoFixture.Create());
        string expectedEndpoint = $"{BuildExpectedEndpoint(libraryId)}?shouldIgnoreThePrefixForAlphaPicker=False";

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicArtistLiteDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSearchTermHasSpecialCharacters_ShouldEscapeIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        const string SEARCH_TERM = "rock & roll";
        GetMusicArtistsLiteRequest request = _getMusicArtistsLiteRequestFixture.Create(
            libraryId: libraryId,
            searchTerm: SEARCH_TERM,
            includeCurrentPage: false,
            includePerPage: false,
            includeFilterAlphaKey: false,
            includeSortBy: false,
            includeSortOrder: false);
        _mockApiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicArtistLiteDtoFixture.Create());
        string expectedEndpoint = $"{BuildExpectedEndpoint(libraryId)}?searchTerm={Uri.EscapeDataString(SEARCH_TERM)}&shouldIgnoreThePrefixForAlphaPicker=False";

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicArtistLiteDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSearchTermAndAlphaKeyAndSortByAreWhitespace_ShouldOmitThem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetMusicArtistsLiteRequest request = _getMusicArtistsLiteRequestFixture.Create(
            libraryId: libraryId,
            searchTerm: "   ",
            filterAlphaKey: " ",
            sortBy: string.Empty,
            includeCurrentPage: false,
            includePerPage: false,
            includeSortOrder: false);
        _mockApiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicArtistLiteDtoFixture.Create());
        string expectedEndpoint = $"{BuildExpectedEndpoint(libraryId)}?shouldIgnoreThePrefixForAlphaPicker=False";

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicArtistLiteDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyCurrentPageIsProvided_ShouldAppendOnlyCurrentPageAndTheIgnorePrefixFlag()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetMusicArtistsLiteRequest request = _getMusicArtistsLiteRequestFixture.Create(
            libraryId: libraryId,
            currentPage: 3,
            includePerPage: false,
            includeSearchTerm: false,
            includeFilterAlphaKey: false,
            includeSortBy: false,
            includeSortOrder: false);
        _mockApiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicArtistLiteDtoFixture.Create());
        string expectedEndpoint = $"{BuildExpectedEndpoint(libraryId)}?currentPage=3&shouldIgnoreThePrefixForAlphaPicker=False";

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicArtistLiteDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPaginatedArtists_ShouldReturnSuccessJsonWithArtists()
    {
        // Arrange
        GetMusicArtistsLiteRequest request = _getMusicArtistsLiteRequestFixture.Create();
        PaginatedMusicArtistLiteDto expectedResponse = _paginatedMusicArtistLiteDtoFixture.Create(
            data: [_musicArtistLiteDtoFixture.Create(name: "Test Artist")],
            currentPage: 1,
            perPage: 20,
            count: 1,
            numberOfPages: 1);
        _mockApiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(1, data.GetProperty("data").GetArrayLength());
    }

    /// <summary>
    /// Builds the upstream endpoint the artist retrieval is expected to be sent to, without query parameters.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are retrieved.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId)
    {
        return ApiRoutes.Music.GET_ARTISTS_LITE.Replace("{libraryId}", libraryId.ToString());
    }
}
