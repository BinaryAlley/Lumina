#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.MusicBrainz.Core;
using Lumina.Plugins.MusicBrainz.Core.Api;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.UnitTests.Common.TestHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzArtistMetadataProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzArtistMetadataProviderTests
{
    private static readonly Guid s_artistId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ArtistMetadataLookupDtoFixture _artistMetadataLookupDtoFixture = new();
    private readonly MusicBrainzSettingsDtoFixture _musicBrainzSettingsDtoFixture = new();

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Arrange
        (MusicBrainzArtistMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        string result = sut.Name;

        // Assert
        Assert.Equal("MusicBrainz", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Arrange
        (MusicBrainzArtistMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        IReadOnlyList<LibraryType> result = sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        (MusicBrainzArtistMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        bool result = sut.RequiresWebAccess;

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetSearchResultsAsync_WhenTheNameIsMissing_ShouldReturnAnEmptyCollectionWithoutAnyRequest(string? name)
    {
        // Arrange
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json("{}"));
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: "path", name: name);

        // Act
        IReadOnlyList<ArtistMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheSearchReturnsArtists_ShouldMapEveryNamedArtistAndSkipTheUnnamedOnes()
    {
        // Arrange
        const string SEARCH_JSON = """
        {
            "count": 3,
            "artists": [
                { "id": "11111111-1111-1111-1111-111111111111", "name": "The Artist" },
                { "id": "22222222-2222-2222-2222-222222222222", "name": "Other Artist" },
                { "id": "33333333-3333-3333-3333-333333333333", "name": null }
            ]
        }
        """;
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: "path", name: "  The Artist  ");

        // Act
        IReadOnlyList<ArtistMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("The Artist", result[0].Name);
        Assert.Equal("Other Artist", result[1].Name);
        Assert.Equal(1, handler.CallCount);
        // the name is trimmed and the configured search result limit is used
        Assert.Contains("limit=10", handler.Requests[0].RequestUri!.Query);
        Assert.Contains("query=The%20Artist", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheSearchResponseIsEmpty_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":0,"artists":[]}""";
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: "path", name: "The Artist");

        // Act
        IReadOnlyList<ArtistMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheMusicBrainzIdentifierIsPresent_ShouldLookUpTheArtistByIdentifier()
    {
        // Arrange
        string artistJson = CreateArtistJson("The Artist", "Group", "11111111-1111-1111-1111-111111111111");
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(artistJson));
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: "path", musicBrainzArtistId: s_artistId);

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Artist", result.Name);
        Assert.Equal(MusicArtistType.Group, result.Type);
        Assert.Equal(s_artistId, result.MusicBrainzArtistId);
        Assert.Equal(1, handler.CallCount);
        Assert.Contains($"/artist/{s_artistId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheIdentifierLookupReturnsNothing_ShouldReturnNull()
    {
        // Arrange
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: "path", musicBrainzArtistId: s_artistId);

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenThereIsNeitherAnIdentifierNorAName_ShouldReturnNullWithoutAnyRequest()
    {
        // Arrange
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json("{}"));
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: "path", name: "   ");

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheNameSearchReturnsNothing_ShouldReturnNull()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":0,"artists":[]}""";
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(_artistMetadataLookupDtoFixture.Create(path: "path", name: "The Artist"), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheSearchResultHasNoIdentifier_ShouldReturnNull()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"artists":[{"id":null,"name":"The Artist"}]}""";
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(_artistMetadataLookupDtoFixture.Create(path: "path", name: "The Artist"), CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheSearchResultIdentifierIsNotAGuid_ShouldMapTheSearchResult()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"artists":[{"id":"not-a-guid","name":"The Artist","type":"Group"}]}""";
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(_artistMetadataLookupDtoFixture.Create(path: "path", name: "The Artist"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Artist", result.Name);
        Assert.Equal(MusicArtistType.Group, result.Type);
        Assert.Null(result.MusicBrainzArtistId);
        // the search is used, and the per identifier lookup must not be requested
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheSearchResultIdentifierIsAGuid_ShouldResolveTheDetailedArtist()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"artists":[{"id":"11111111-1111-1111-1111-111111111111","name":"The Artist"}]}""";
        string detailJson = CreateArtistJson("The Detailed Artist", "Person", "11111111-1111-1111-1111-111111111111");
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => request.RequestUri!.AbsolutePath.Contains("/artist/") ? Json(detailJson) : Json(SEARCH_JSON));

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(_artistMetadataLookupDtoFixture.Create(path: "path", name: "The Artist"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Detailed Artist", result.Name);
        Assert.Equal(MusicArtistType.Person, result.Type);
        Assert.Equal(2, handler.CallCount);
        Assert.Contains("limit=1", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheDetailedArtistLookupReturnsNothing_ShouldMapTheSearchResult()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"artists":[{"id":"11111111-1111-1111-1111-111111111111","name":"The Artist","type":"Group"}]}""";
        (MusicBrainzArtistMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => request.RequestUri!.AbsolutePath.Contains("/artist/") ? new HttpResponseMessage(HttpStatusCode.NotFound) : Json(SEARCH_JSON));

        // Act
        ArtistMetadataDto? result = await sut.GetMetadataAsync(_artistMetadataLookupDtoFixture.Create(path: "path", name: "The Artist"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Artist", result.Name);
        Assert.Equal(MusicArtistType.Group, result.Type);
        Assert.Equal(2, handler.CallCount);
    }

    /// <summary>
    /// Creates a <see cref="MusicBrainzArtistMetadataProvider"/> whose requests are answered by the provided stub handler.
    /// </summary>
    /// <param name="responseFactory">The factory that produces the response of a request.</param>
    /// <returns>The created provider and its stub handler.</returns>
    private (MusicBrainzArtistMetadataProvider Provider, StubHttpMessageHandler Handler) CreateProvider(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory)
    {
        StubHttpMessageHandler handler = new(responseFactory);
        MusicBrainzSettingsProvider settingsProvider = new(null, MusicBrainzPlugin.s_pluginId, _musicBrainzSettingsDtoFixture.Create(
            baseUrl: "http://localhost/",
            searchResultLimit: 10,
            releaseLookupLimit: 25,
            minimumRequestInterval: TimeSpan.Zero));
        MusicBrainzHttpClient httpClient = new(new HttpClient(handler), settingsProvider, new MusicBrainzRequestThrottle(), new MusicBrainzResponseCache());
        return (new MusicBrainzArtistMetadataProvider(httpClient, settingsProvider), handler);
    }

    /// <summary>
    /// Creates the JSON of an artist response.
    /// </summary>
    /// <param name="name">The name of the artist.</param>
    /// <param name="type">The MusicBrainz type of the artist.</param>
    /// <param name="id">The MusicBrainz identifier of the artist.</param>
    /// <returns>The JSON of the artist response.</returns>
    private static string CreateArtistJson(string name, string type, string id)
    {
        return $$"""{"id":"{{id}}","name":"{{name}}","type":"{{type}}"}""";
    }

    /// <summary>
    /// Creates an OK response with the provided JSON body.
    /// </summary>
    /// <param name="json">The JSON body of the response.</param>
    /// <returns>The created response.</returns>
    private static HttpResponseMessage Json(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
