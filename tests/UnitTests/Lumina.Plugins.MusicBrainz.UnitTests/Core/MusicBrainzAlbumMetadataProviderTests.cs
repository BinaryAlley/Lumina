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
/// Contains unit tests for the <see cref="MusicBrainzAlbumMetadataProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzAlbumMetadataProviderTests
{
    private static readonly Guid s_releaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_releaseGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly AlbumMetadataLookupDtoFixture _albumMetadataLookupDtoFixture = new();
    private readonly MusicBrainzSettingsDtoFixture _musicBrainzSettingsDtoFixture = new();

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        string result = sut.Name;

        // Assert
        Assert.Equal("MusicBrainz", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        IReadOnlyList<LibraryType> result = sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        bool result = sut.RequiresWebAccess;

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetSearchResultsAsync_WhenTheTitleIsMissing_ShouldReturnAnEmptyCollectionWithoutAnyRequest(string? title)
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json("{}"));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", title: title);

        // Act
        IReadOnlyList<AlbumMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheSearchReturnsReleaseGroups_ShouldMapEveryTitledOneAndSkipTheUntitledOnes()
    {
        // Arrange
        const string SEARCH_JSON = """
        {
            "count": 3,
            "release-groups": [
                { "id": "22222222-2222-2222-2222-222222222222", "title": "The Album", "primary-type": "Album" },
                { "id": "33333333-3333-3333-3333-333333333333", "title": "Other Album" },
                { "id": "44444444-4444-4444-4444-444444444444", "title": null }
            ]
        }
        """;
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", title: "  The \"Album\"  ", artistName: "  The Artist  ");

        // Act
        IReadOnlyList<AlbumMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("The Album", result[0].Title);
        Assert.Equal("Other Album", result[1].Title);
        Assert.Equal(1, handler.CallCount);
        Assert.Contains("limit=10", handler.Requests[0].RequestUri!.Query);
        // the quotes are removed from the built query, and the artist constraint is appended
        string query = Uri.UnescapeDataString(handler.Requests[0].RequestUri!.Query);
        Assert.Contains("releasegroup:\"The Album\"", query);
        Assert.Contains("AND artist:\"The Artist\"", query);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseAndTheArtistArePresent_ShouldMapBothTheReleaseGroupAndTheRelease()
    {
        // Arrange
        string releaseJson = ReleaseJson(s_releaseId, "The Release", s_releaseGroupId, "The Album", "1975-06-01", "Official", "1975-11-21", "US");
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : releaseJson));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Album", result.Title);
        Assert.Equal(s_releaseId, result.MusicBrainzReleaseId);
        Assert.Equal(s_releaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal("The Release", result.ReleaseTitle);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseIdentifierIsPresentButTheReleaseIsNotFound_ShouldReturnNull()
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseCarriesAnUnparsableReleaseGroupIdentifier_ShouldMapOnlyTheRelease()
    {
        // Arrange
        const string RELEASE_JSON = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "title": "The Release",
            "release-group": { "id": "not-a-guid", "title": "The Album" }
        }
        """;
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(RELEASE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Release", result.Title);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseDoesNotReferenceAReleaseGroup_ShouldMapOnlyTheRelease()
    {
        // Arrange
        const string RELEASE_JSON = """{"id":"11111111-1111-1111-1111-111111111111","title":"The Release"}""";
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(RELEASE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Release", result.Title);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenThereIsNeitherAnIdentifierNorATitle_ShouldReturnNullWithoutAnyRequest()
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json("{}"));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", title: "   ");

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOnlyTheTitleIsPresent_ShouldSearchAndResolveTheFirstReleaseGroup()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"release-groups":[{"id":"22222222-2222-2222-2222-222222222222","title":"The Album"}]}""";
        const string BROWSE_JSON = """{"release-count":0,"releases":[]}""";
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(Route(request, SEARCH_JSON, releaseGroupJson, BROWSE_JSON)));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", title: "The Album");

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Album", result.Title);
        Assert.Equal(s_releaseGroupId, result.MusicBrainzReleaseGroupId);
        // the search, the detail lookup and the release browse are all used
        Assert.Equal(3, handler.CallCount);
        Assert.Contains("limit=1", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTitleSearchReturnsNothing_ShouldReturnNull()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":0,"release-groups":[]}""";
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", title: "The Album");

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheSearchResultIdentifierIsNotAGuid_ShouldReturnNull()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"release-groups":[{"id":"not-a-guid","title":"The Album"}]}""";
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", title: "The Album");

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseGroupIdentifierIsPresent_ShouldFetchItDirectly()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseGroupJson));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_releaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Contains($"/release-group/{s_releaseGroupId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseGroupIsUnknownButATitleIsPresent_ShouldFallBackToTheTitleSearch()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"release-groups":[{"id":"22222222-2222-2222-2222-222222222222","title":"The Album"}]}""";
        const string BROWSE_JSON = """{"release-count":0,"releases":[]}""";
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.Query.Contains("query=")
                ? Json(SEARCH_JSON)
                : request.RequestUri.AbsolutePath.Contains("/release-group/")
                    ? callIndex == 1
                        ? new HttpResponseMessage(HttpStatusCode.NotFound)
                        : Json(releaseGroupJson)
                    : Json(BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, title: "The Album");

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Album", result.Title);
        Assert.Equal(s_releaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseGroupIdentifierIsPresentButTheDetailIsMissing_ShouldReturnNull()
    {
        // Arrange
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheLocalTrackCountIsUnknown_ShouldNotSelectAnyRelease()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseGroupJson));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: null);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.MusicBrainzReleaseId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenNoReleaseMatchesTheLocalTrackCount_ShouldNotSelectAnyRelease()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        const string BROWSE_JSON = """{"release-count":1,"releases":[{"id":"11111111-1111-1111-1111-111111111111","title":"The Release","media":[{"position":1,"track-count":9}]}]}""";
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: 12);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.MusicBrainzReleaseId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenSeveralReleasesMatch_ShouldSelectTheBestScoredOne()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        const string BROWSE_JSON = """
        {
            "release-count": 3,
            "releases": [
                { "id": "11111111-1111-1111-1111-111111111111", "title": "Bootleg Edition", "status": "Bootleg", "date": "1975", "media": [ { "position": 1, "track-count": 2 } ] },
                { "id": "22222222-2222-2222-2222-222222222222", "title": "Official Edition", "status": "Official", "date": "1975-11-21", "barcode": "123", "label-info": [ { "catalog-number": "CAT" } ], "media": [ { "position": 1, "track-count": 2 } ] },
                { "id": "33333333-3333-3333-3333-333333333333", "title": "Promotion Edition", "status": "Promotion", "date": "1980-01-01", "media": [ { "position": 1, "track-count": 2 } ] }
            ]
        }
        """;
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: 2, releaseYear: 1975);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), result.MusicBrainzReleaseId);
        Assert.Equal("Official Edition", result.ReleaseTitle);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenSeveralReleasesScoreTheSame_ShouldSelectTheFirstOne()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        const string BROWSE_JSON = """
        {
            "release-count": 2,
            "releases": [
                { "id": "11111111-1111-1111-1111-111111111111", "title": "First Edition", "status": "Official", "date": "1975-11-21", "media": [ { "position": 1, "track-count": 2 } ] },
                { "id": "22222222-2222-2222-2222-222222222222", "title": "Second Edition", "status": "Official", "date": "1975-11-21", "media": [ { "position": 1, "track-count": 2 } ] }
            ]
        }
        """;
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: 2, releaseYear: 1975);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), result.MusicBrainzReleaseId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseDateIsUnparsable_ShouldStillSelectTheMatchingRelease()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        const string BROWSE_JSON = """
        {
            "release-count": 2,
            "releases": [
                { "id": "11111111-1111-1111-1111-111111111111", "title": "Unparsable Edition", "status": "Official", "date": "not a date", "media": [ { "position": 1, "track-count": 2 } ] },
                { "id": "22222222-2222-2222-2222-222222222222", "title": "Matching Edition", "status": "Official", "date": "1975-06-01", "media": [ { "position": 1, "track-count": 2 } ] }
            ]
        }
        """;
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: 2, releaseYear: 1975);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), result.MusicBrainzReleaseId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseCarriesSeveralCatalogNumbers_ShouldMapAllOfThem()
    {
        // Arrange
        const string RELEASE_JSON = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "title": "A Night at the Opera",
            "release-group": { "id": "22222222-2222-2222-2222-222222222222", "title": "A Night at the Opera", "primary-type": "Album" },
            "label-info": [
                { "catalog-number": "EMC 4008", "label": { "name": "EMI" } },
                { "catalog-number": "0C 066-98 485", "label": { "name": "EMI" } },
                { "catalog-number": "EMC 4008", "label": { "name": "EMI" } }
            ]
        }
        """;
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "A Night at the Opera", "Album", "1975-11-21");
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : RELEASE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.CatalogNumbers);
        // every distinct catalog number is kept, while the duplicates are removed.
        Assert.Equal(["EMC 4008", "0C 066-98 485"], result.CatalogNumbers);
        Assert.Equal("EMI", result.Label);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheMatchingReleaseHasNoMedia_ShouldStillSelectIt()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        const string BROWSE_JSON = """{"release-count":1,"releases":[{"id":"11111111-1111-1111-1111-111111111111","title":"Empty Edition","status":"Official","media":[]}]}""";
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: 0);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), result.MusicBrainzReleaseId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseDatesAreMissingOrTooShort_ShouldStillSelectAMatchingRelease()
    {
        // Arrange
        string releaseGroupJson = ReleaseGroupJson(s_releaseGroupId, "The Album", "Album", "1975-06-01");
        const string BROWSE_JSON = """
        {
            "release-count": 2,
            "releases": [
                { "id": "11111111-1111-1111-1111-111111111111", "status": "Official", "media": [ { "position": 1, "track-count": 2 } ] },
                { "id": "22222222-2222-2222-2222-222222222222", "status": "Official", "date": "75", "media": [ { "position": 1, "track-count": 2 } ] }
            ]
        }
        """;
        (MusicBrainzAlbumMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(request.RequestUri!.AbsolutePath.Contains("/release-group/") ? releaseGroupJson : BROWSE_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId, trackCount: 2, releaseYear: 1975);

        // Act
        AlbumMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), result.MusicBrainzReleaseId);
    }

    /// <summary>
    /// Creates a <see cref="MusicBrainzAlbumMetadataProvider"/> whose requests are answered by the provided stub handler.
    /// </summary>
    /// <param name="responseFactory">The factory that produces the response of a request.</param>
    /// <returns>The created provider and its stub handler.</returns>
    private (MusicBrainzAlbumMetadataProvider Provider, StubHttpMessageHandler Handler) CreateProvider(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory)
    {
        StubHttpMessageHandler handler = new(responseFactory);
        MusicBrainzSettingsProvider settingsProvider = new(null, MusicBrainzPlugin.s_pluginId, _musicBrainzSettingsDtoFixture.Create(
            baseUrl: "http://localhost/",
            searchResultLimit: 10,
            releaseLookupLimit: 25,
            minimumRequestInterval: TimeSpan.Zero));
        MusicBrainzHttpClient httpClient = new(new HttpClient(handler), settingsProvider, new MusicBrainzRequestThrottle(), new MusicBrainzResponseCache());
        return (new MusicBrainzAlbumMetadataProvider(httpClient, settingsProvider), handler);
    }

    /// <summary>
    /// Routes a request to the search, release group or release browse JSON, based on the request URI.
    /// </summary>
    /// <param name="request">The request to route.</param>
    /// <param name="searchString">The JSON returned for a search request.</param>
    /// <param name="releaseGroupJson">The JSON returned for a release group detail request.</param>
    /// <param name="browseJson">The JSON returned for a release browse request.</param>
    /// <returns>The JSON that answers the request.</returns>
    private static string Route(HttpRequestMessage request, string searchString, string releaseGroupJson, string browseJson)
    {
        if (request.RequestUri!.Query.Contains("query=", StringComparison.Ordinal))
            return searchString;
        return request.RequestUri.AbsolutePath.Contains("/release-group/", StringComparison.Ordinal) ? releaseGroupJson : browseJson;
    }

    /// <summary>
    /// Creates the JSON of a release group response.
    /// </summary>
    /// <param name="id">The MusicBrainz identifier of the release group.</param>
    /// <param name="title">The title of the release group.</param>
    /// <param name="primaryType">The primary type of the release group.</param>
    /// <param name="firstReleaseDate">The first release date of the release group.</param>
    /// <returns>The JSON of the release group response.</returns>
    private static string ReleaseGroupJson(Guid id, string title, string primaryType, string firstReleaseDate)
    {
        return $$"""{"id":"{{id:D}}","title":"{{title}}","primary-type":"{{primaryType}}","first-release-date":"{{firstReleaseDate}}"}""";
    }

    /// <summary>
    /// Creates the JSON of a release response.
    /// </summary>
    /// <param name="id">The MusicBrainz identifier of the release.</param>
    /// <param name="title">The title of the release.</param>
    /// <param name="releaseGroupId">The MusicBrainz identifier of the release group.</param>
    /// <param name="releaseGroupTitle">The title of the release group.</param>
    /// <param name="releaseGroupFirstReleaseDate">The first release date of the release group.</param>
    /// <param name="date">The date the release was issued.</param>
    /// <param name="status">The status of the release.</param>
    /// <param name="releaseDate">The date of the release.</param>
    /// <param name="country">The country the release was issued in.</param>
    /// <returns>The JSON of the release response.</returns>
    private static string ReleaseJson(Guid id, string title, Guid releaseGroupId, string releaseGroupTitle, string releaseGroupFirstReleaseDate, string status, string releaseDate, string country)
    {
        return $$"""
        {
            "id": "{{id:D}}",
            "title": "{{title}}",
            "status": "{{status}}",
            "date": "{{releaseDate}}",
            "country": "{{country}}",
            "release-group": { "id": "{{releaseGroupId:D}}", "title": "{{releaseGroupTitle}}", "primary-type": "Album", "first-release-date": "{{releaseGroupFirstReleaseDate}}" }
        }
        """;
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
