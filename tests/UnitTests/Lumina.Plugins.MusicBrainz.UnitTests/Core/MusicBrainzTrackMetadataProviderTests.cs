#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
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
/// Contains unit tests for the <see cref="MusicBrainzTrackMetadataProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzTrackMetadataProviderTests
{
    private static readonly Guid s_releaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_recordingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid s_secondRecordingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid s_workId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly TrackMetadataLookupDtoFixture _trackMetadataLookupDtoFixture = new();
    private readonly MusicBrainzSettingsDtoFixture _musicBrainzSettingsDtoFixture = new();

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Arrange
        (MusicBrainzTrackMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        string result = sut.Name;

        // Assert
        Assert.Equal("MusicBrainz", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Arrange
        (MusicBrainzTrackMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

        // Act
        IReadOnlyList<LibraryType> result = sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        (MusicBrainzTrackMetadataProvider sut, _) = CreateProvider((request, callIndex) => Json("{}"));

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
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json("{}"));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", title: title);

        // Act
        IReadOnlyList<AudioMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheSearchReturnsRecordings_ShouldMapEveryTitledOneAndSkipTheUntitledOnes()
    {
        // Arrange
        const string SEARCH_JSON = """
        {
            "count": 3,
            "recordings": [
                { "id": "22222222-2222-2222-2222-222222222222", "title": "The Track" },
                { "id": "33333333-3333-3333-3333-333333333333", "title": "Other Track" },
                { "id": "44444444-4444-4444-4444-444444444444", "title": null }
            ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", title: "  The \"Track\"  ", artistName: "The Artist", releaseName: "The Album");

        // Act
        IReadOnlyList<AudioMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("The Track", result[0].Title);
        Assert.Equal("Other Track", result[1].Title);
        Assert.Equal(1, handler.CallCount);
        Assert.Contains("limit=10", handler.Requests[0].RequestUri!.Query);
        string query = Uri.UnescapeDataString(handler.Requests[0].RequestUri!.Query);
        Assert.Contains("recording:\"The Track\"", query);
        Assert.Contains("artist:\"The Artist\"", query);
        Assert.Contains("release:\"The Album\"", query);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseTracklistMatchesTheRecordingIdentifier_ShouldResolveTheTrackFromTheRelease()
    {
        // Arrange
        string releaseJson = TwoTrackReleaseJson();
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: s_secondRecordingId, trackNumber: 9, discNumber: 1);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_secondRecordingId, result.MusicBrainzRecordingId);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseTracklistDoesNotMatch_ShouldFallBackToTheRecordingIdentifier()
    {
        // Arrange
        Guid fallbackRecordingId = Guid.NewGuid();
        string releaseJson = TwoTrackReleaseJson();
        string recordingJson = RecordingJson(fallbackRecordingId, "Searched Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(
            request.RequestUri!.AbsolutePath.Contains("/release/") ? releaseJson : recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: fallbackRecordingId, trackNumber: 99);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        // The release has no matching track, so it falls through to the recording lookup.
        Assert.NotNull(result);
        Assert.Equal(fallbackRecordingId, result.MusicBrainzRecordingId);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseIsNotFound_ShouldFallBackToTheRecordingIdentifier()
    {
        // Arrange
        string recordingJson = RecordingJson(s_recordingId, "Searched Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.AbsolutePath.Contains("/release/") ? new HttpResponseMessage(HttpStatusCode.NotFound) : Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_recordingId, result.MusicBrainzRecordingId);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheMatchedTrackHasNoRecording_ShouldFallThroughToTheSearch()
    {
        // Arrange
        const string RELEASE_JSON = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 1, "tracks": [ { "position": 1, "title": "Track Without Recording" } ] } ]
        }
        """;
        const string SEARCH_JSON = """{"count":1,"recordings":[{"id":"22222222-2222-2222-2222-222222222222","title":"Searched Track"}]}""";
        string recordingJson = RecordingJson(s_recordingId, "Searched Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.Query.Contains("query=")
                ? Json(SEARCH_JSON)
                : request.RequestUri.AbsolutePath.Contains("/recording/")
                    ? Json(recordingJson)
                    : Json(RELEASE_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, title: "Searched Track", trackNumber: 1, discNumber: 1);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Searched Track", result.Title);
        Assert.Equal(s_recordingId, result.MusicBrainzRecordingId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseTrackCarriesAnInlineWork_ShouldUseItWithoutAFurtherLookup()
    {
        // Arrange
        string releaseJson = $$"""
        {
            "id": "{{s_releaseId:D}}",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 1, "tracks": [ { "position": 1, "title": "The Track", "recording": { "id": "{{s_recordingId:D}}", "title": "The Track", "relations": [ { "target-type": "work", "work": { "id": "{{s_workId:D}}", "title": "Inline Work", "type": "Song" } } ] } } ] } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Work);
        Assert.Equal(s_workId, result.Work.MusicBrainzWorkId);
        Assert.Equal("Inline Work", result.Work.Title);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheInlineWorkRelationHasNoWorkPayload_ShouldFallBackToTheWorkIdentifierOfTheTags()
    {
        // Arrange
        string releaseJson = $$"""
        {
            "id": "{{s_releaseId:D}}",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 1, "tracks": [ { "position": 1, "title": "The Track", "recording": { "id": "{{s_recordingId:D}}", "title": "The Track", "relations": [ { "target-type": "work" } ] } } ] } ]
        }
        """;
        string workJson = $$"""{"id":"{{s_workId:D}}","title":"Tagged Work"}""";
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(
            request.RequestUri!.AbsolutePath.Contains("/release/") ? releaseJson : workJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: s_recordingId, musicBrainzWorkId: s_workId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Work);
        Assert.Equal(s_workId, result.Work.MusicBrainzWorkId);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseTrackCarriesAWorkWithoutAWorkTitleInTheTags_ShouldNotQueryTheWorkAgain()
    {
        // Arrange
        string releaseJson = $$"""
        {
            "id": "{{s_releaseId:D}}",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 1, "tracks": [ { "position": 1, "title": "The Track", "recording": { "id": "{{s_recordingId:D}}", "title": "The Track", "relations": [ { "target-type": "work", "work": { "id": "{{s_workId:D}}", "title": "Inline Work" } } ] } } ] } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: s_recordingId, musicBrainzWorkId: Guid.NewGuid());

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Inline Work", result.Work!.Title);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOnlyTheRecordingIdentifierIsPresent_ShouldResolveTheRecordingItsWorkAndItsRelease()
    {
        // Arrange
        string recordingJson = $$"""
        {
            "id": "{{s_recordingId:D}}",
            "title": "The Track",
            "relations": [ { "target-type": "work", "work": { "id": "{{s_workId:D}}", "title": "The Work" } } ],
            "releases": [ { "id": "{{s_releaseId:D}}", "title": "The Release", "text-representation": { "language": "eng", "script": "Latn" } } ]
        }
        """;
        string workJson = $$"""{"id":"{{s_workId:D}}","title":"The Work","type":"Song"}""";
        string releaseJson = $$"""{"id":"{{s_releaseId:D}}","title":"The Release","text-representation":{"language":"eng","script":"Latn"} }""";
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.AbsolutePath.Contains("/work/") ? Json(workJson)
            : request.RequestUri.AbsolutePath.Contains("/release/") ? Json(releaseJson)
            : Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_recordingId, result.MusicBrainzRecordingId);
        Assert.Equal(s_workId, result.Work!.MusicBrainzWorkId);
        Assert.Equal("The Work", result.Work.Title);
        Assert.NotNull(result.Language);
        Assert.Equal("en", result.Language.LanguageCode);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheRecordingRelationsCarryAnUnparsableWorkIdentifier_ShouldUseTheEmbeddedWork()
    {
        // Arrange
        string recordingJson = $$"""
        {
            "id": "{{s_recordingId:D}}",
            "title": "The Track",
            "relations": [ { "target-type": "work", "work": { "id": "not-a-guid", "title": "Embedded Work" } } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Embedded Work", result.Work!.Title);
        Assert.Null(result.Work.MusicBrainzWorkId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheRecordingHasNoWorkRelation_ShouldReturnATrackWithoutWork()
    {
        // Arrange
        string recordingJson = RecordingJson(s_recordingId, "The Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Work);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheRecordingReferencesAnUnparsableRelease_ShouldReturnATrackWithoutRelease()
    {
        // Arrange
        string recordingJson = $$"""
        {
            "id": "{{s_recordingId:D}}",
            "title": "The Track",
            "releases": [ { "id": "not-a-guid", "title": "The Release" } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Language);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheRecordingHasNoReleases_ShouldReturnATrackWithoutRelease()
    {
        // Arrange
        string recordingJson = RecordingJson(s_recordingId, "The Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Script);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheIsrcMatchesARecordingWithAParsableIdentifier_ShouldResolveItInFull()
    {
        // Arrange
        const string ISRC_JSON = """{"recordings":[{"id":"22222222-2222-2222-2222-222222222222","title":"Isrc Track"}]}""";
        string recordingJson = RecordingJson(s_recordingId, "Resolved Isrc Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.AbsolutePath.Contains("/isrc/") ? Json(ISRC_JSON) : Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", isrc: "US1A2B3C4D5E");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resolved Isrc Track", result.Title);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheIsrcMatchesARecordingWithAnUnparsableIdentifier_ShouldReturnItAsIs()
    {
        // Arrange
        const string ISRC_JSON = """{"recordings":[{"id":"not-a-guid","title":"Raw Isrc Track"}]}""";
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(ISRC_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", isrc: "US1A2B3C4D5E");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Raw Isrc Track", result.Title);
        Assert.Null(result.MusicBrainzRecordingId);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheIsrcMatchesNothing_ShouldReturnNullWithoutATitle()
    {
        // Arrange
        const string ISRC_JSON = """{"recordings":[]}""";
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(ISRC_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", isrc: "US1A2B3C4D5E");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOnlyTheTitleIsPresent_ShouldSearchAndResolveTheRecordingInFull()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"recordings":[{"id":"22222222-2222-2222-2222-222222222222","title":"Searched Track"}]}""";
        string recordingJson = RecordingJson(s_recordingId, "Resolved Searched Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.Query.Contains("query=") ? Json(SEARCH_JSON) : Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", title: "Searched Track");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resolved Searched Track", result.Title);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheSearchReturnsNothing_ShouldReturnNull()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":0,"recordings":[]}""";
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(SEARCH_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", title: "Searched Track");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenNoLookupInformationIsPresent_ShouldReturnNullWithoutAnyRequest()
    {
        // Arrange
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json("{}"));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheLocalDiscNumberDoesNotMatchAnyMedium_ShouldReturnNull()
    {
        // Arrange
        string releaseJson = TwoTrackReleaseJson();
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, discNumber: 9);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOnlyTheTrackNumberIsPresent_ShouldSelectTheTrackOnAnyDisc()
    {
        // Arrange
        string releaseJson = TwoTrackReleaseJson();
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, trackNumber: 2);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_secondRecordingId, result.MusicBrainzRecordingId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTitleSinglesOutATrack_ShouldSelectIt()
    {
        // Arrange
        string releaseJson = TwoTrackReleaseJson();
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(releaseJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, title: "Second Track");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_secondRecordingId, result.MusicBrainzRecordingId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenSeveralTracksShareTheTitle_ShouldUseTheDurationAsATieBreaker()
    {
        // Arrange
        const string RELEASE_JSON = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 2, "tracks": [
                { "position": 1, "title": "Same Title", "length": 100000, "recording": { "id": "22222222-2222-2222-2222-222222222222", "title": "Same Title" } },
                { "position": 2, "title": "Same Title", "length": 200000, "recording": { "id": "33333333-3333-3333-3333-333333333333", "title": "Same Title" } }
            ] } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(RELEASE_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, title: "Same Title", durationInSeconds: 190);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_secondRecordingId, result.MusicBrainzRecordingId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenSeveralTracksShareTheTitleAndNoDurationIsKnown_ShouldSelectTheFirstOne()
    {
        // Arrange
        const string RELEASE_JSON = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 2, "tracks": [
                { "position": 1, "title": "Same Title", "length": 100000, "recording": { "id": "22222222-2222-2222-2222-222222222222", "title": "Same Title" } },
                { "position": 2, "title": "Same Title", "length": 200000, "recording": { "id": "33333333-3333-3333-3333-333333333333", "title": "Same Title" } }
            ] } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(RELEASE_JSON));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, title: "Different Title");

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(s_recordingId, result.MusicBrainzRecordingId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryAWorkIdButMusicBrainzAlreadyLinkedAWork_ShouldResolveTheMusicBrainzWorkInFull()
    {
        // Arrange
        string recordingJson = $$"""
        {
            "id": "{{s_recordingId:D}}",
            "title": "The Track",
            "relations": [ { "target-type": "work", "work": { "id": "{{s_workId:D}}", "title": "MusicBrainz Work" } } ]
        }
        """;
        string workJson = $$"""{"id":"{{s_workId:D}}","title":"Tagged Work"}""";
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(
            request.RequestUri!.AbsolutePath.Contains("/work/") ? workJson : recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId, musicBrainzWorkId: Guid.NewGuid());

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        // The work identifier linked by MusicBrainz is resolved in full, taking precedence over the work identifier of the tags.
        Assert.Equal(s_workId, result.Work!.MusicBrainzWorkId);
        Assert.Equal("Tagged Work", result.Work.Title);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheReleaseTracksHaveMissingOrUnparsableRecordingIdentifiers_ShouldFallBackToTheRecordingLookup()
    {
        // Arrange
        Guid fallbackRecordingId = Guid.NewGuid();
        const string RELEASE_JSON = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 2, "tracks": [
                { "position": 1, "title": "Track Without Recording" },
                { "position": 2, "title": "Track With Bad Recording", "recording": { "id": "not-a-guid", "title": "Track With Bad Recording" } }
            ] } ]
        }
        """;
        string recordingJson = RecordingJson(fallbackRecordingId, "Fallback Track");
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(
            request.RequestUri!.AbsolutePath.Contains("/release/") ? RELEASE_JSON : recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzRecordingId: fallbackRecordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fallbackRecordingId, result.MusicBrainzRecordingId);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenAWorkRelationHasNoWork_ShouldReturnATrackWithoutWork()
    {
        // Arrange
        string recordingJson = $$"""
        {
            "id": "{{s_recordingId:D}}",
            "title": "The Track",
            "relations": [ { "target-type": "work" } ]
        }
        """;
        (MusicBrainzTrackMetadataProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => Json(recordingJson));
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: "path", musicBrainzRecordingId: s_recordingId);

        // Act
        AudioMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Work);
    }

    /// <summary>
    /// Creates a <see cref="MusicBrainzTrackMetadataProvider"/> whose requests are answered by the provided stub handler.
    /// </summary>
    /// <param name="responseFactory">The factory that produces the response of a request.</param>
    /// <returns>The created provider and its stub handler.</returns>
    private (MusicBrainzTrackMetadataProvider Provider, StubHttpMessageHandler Handler) CreateProvider(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory)
    {
        StubHttpMessageHandler handler = new(responseFactory);
        MusicBrainzSettingsProvider settingsProvider = new(null, MusicBrainzPlugin.s_pluginId, _musicBrainzSettingsDtoFixture.Create(
            baseUrl: "http://localhost/",
            searchResultLimit: 10,
            releaseLookupLimit: 25,
            minimumRequestInterval: TimeSpan.Zero));
        MusicBrainzHttpClient httpClient = new(new HttpClient(handler), settingsProvider, new MusicBrainzRequestThrottle(), new MusicBrainzResponseCache());
        return (new MusicBrainzTrackMetadataProvider(httpClient, settingsProvider), handler);
    }

    /// <summary>
    /// Creates the JSON of a release with two tracks, each with its own recording.
    /// </summary>
    /// <returns>The JSON of the release.</returns>
    private static string TwoTrackReleaseJson()
    {
        return $$"""
        {
            "id": "{{s_releaseId:D}}",
            "title": "The Release",
            "media": [ { "position": 1, "track-count": 2, "tracks": [
                { "position": 1, "title": "First Track", "recording": { "id": "{{s_recordingId:D}}", "title": "First Track" } },
                { "position": 2, "title": "Second Track", "recording": { "id": "{{s_secondRecordingId:D}}", "title": "Second Track" } }
            ] } ]
        }
        """;
    }

    /// <summary>
    /// Creates the JSON of a recording response.
    /// </summary>
    /// <param name="id">The MusicBrainz identifier of the recording.</param>
    /// <param name="title">The title of the recording.</param>
    /// <returns>The JSON of the recording response.</returns>
    private static string RecordingJson(Guid id, string title)
    {
        return $$"""{"id":"{{id:D}}","title":"{{title}}"}""";
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