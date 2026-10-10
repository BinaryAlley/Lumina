#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using Lumina.Plugins.ID3.Core.Mapping;
using Lumina.Plugins.ID3.Fixtures.Common.Models.DTO.Tags;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.ID3.UnitTests.Core.Mapping;

/// <summary>
/// Contains unit tests for the <see cref="Id3TagMapper"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3TagMapperTests
{
    private readonly Id3TagDtoFixture _id3TagDtoFixture = new();

    [Fact]
    public void MapArtist_WhenTheArtistNameMatchesATrackArtist_ShouldMapEveryArtistField()
    {
        // Arrange
        Guid trackArtistId = Guid.NewGuid();
        Guid releaseArtistId = Guid.NewGuid();
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["The Artist"],
            albumArtists: ["Some Other Artist"],
            trackArtistSortName: "Artist, The",
            albumArtistSortName: "Artist, Some Other",
            musicBrainzArtistId: trackArtistId.ToString(),
            musicBrainzReleaseArtistId: releaseArtistId.ToString(),
            website: "https://artist.example");

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "The Artist");

        // Assert
        Assert.Equal("The Artist", result.Name);
        Assert.Equal("Artist, The", result.SortName);
        Assert.Equal("https://artist.example", result.Website);
        Assert.Equal(trackArtistId, result.MusicBrainzArtistId);
        Assert.Null(result.Disambiguation);
        Assert.Null(result.Type);
        Assert.Null(result.Gender);
        Assert.Null(result.Country);
        Assert.Null(result.Area);
        Assert.Null(result.BeginArea);
        Assert.Null(result.EndArea);
        Assert.Null(result.LifeSpanBegin);
        Assert.Null(result.LifeSpanEnd);
        Assert.False(result.IsEnded);
        Assert.Null(result.Ipis);
        Assert.Null(result.Isnis);
        Assert.Null(result.Aliases);
        Assert.Null(result.Genres);
        Assert.Null(result.Tags);
        Assert.Null(result.Ratings);
        Assert.Null(result.Contributors);
    }

    [Fact]
    public void MapArtist_WhenTheArtistNameMatchesOnlyAnAlbumArtist_ShouldUseTheAlbumArtistData()
    {
        // Arrange
        Guid trackArtistId = Guid.NewGuid();
        Guid releaseArtistId = Guid.NewGuid();
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Some Other Artist"],
            albumArtists: ["The Artist"],
            trackArtistSortName: "Artist, Some Other",
            albumArtistSortName: "Artist, The",
            musicBrainzArtistId: trackArtistId.ToString(),
            musicBrainzReleaseArtistId: releaseArtistId.ToString());

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "The Artist");

        // Assert
        Assert.Equal("The Artist", result.Name);
        Assert.Equal("Artist, The", result.SortName);
        Assert.Equal(releaseArtistId, result.MusicBrainzArtistId);
    }

    [Fact]
    public void MapArtist_WhenTheArtistNameIsUnknown_ShouldFallBackToTheAlbumArtist()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Track Artist"],
            albumArtists: ["Album Artist"],
            trackArtistSortName: "Artist, Track",
            albumArtistSortName: "Artist, Album");

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "Unknown Artist");

        // Assert
        Assert.Equal("Album Artist", result.Name);
        Assert.Equal("Artist, Album", result.SortName);
    }

    [Fact]
    public void MapArtist_WhenTheArtistNameIsUnknownAndThereAreNoAlbumArtists_ShouldFallBackToTheTrackArtist()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Track Artist"],
            albumArtists: []);

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "Unknown Artist");

        // Assert
        Assert.Equal("Track Artist", result.Name);
    }

    [Fact]
    public void MapArtist_WhenTheTrackArtistSortNameIsMissing_ShouldFallBackToTheAlbumArtistSortName()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["The Artist"],
            albumArtists: ["Some Other Artist"],
            includeTrackArtistSortName: false,
            albumArtistSortName: "Artist, Album");

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "The Artist");

        // Assert
        Assert.Equal("Artist, Album", result.SortName);
    }

    [Fact]
    public void MapArtist_WhenTheAlbumArtistSortNameIsMissing_ShouldFallBackToTheTrackArtistSortName()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Some Other Artist"],
            albumArtists: ["The Artist"],
            trackArtistSortName: "Artist, Track",
            includeAlbumArtistSortName: false);

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "The Artist");

        // Assert
        Assert.Equal("Artist, Track", result.SortName);
    }

    [Fact]
    public void MapArtist_WhenTheUnknownArtistHasOnlyATrackArtistSortName_ShouldUseIt()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Track Artist"],
            albumArtists: [],
            trackArtistSortName: "Artist, Track",
            includeAlbumArtistSortName: false);

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "Unknown Artist");

        // Assert
        Assert.Equal("Track Artist", result.Name);
        Assert.Equal("Artist, Track", result.SortName);
    }

    [Fact]
    public void MapArtist_WhenTheArtistNameIsWhiteSpace_ShouldFallBackToTheCreditedArtists()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Track Artist"],
            albumArtists: ["Album Artist"]);

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "   ");

        // Assert
        Assert.Equal("Album Artist", result.Name);
    }

    [Fact]
    public void MapArtist_WhenTheIdentifierCannotBeParsed_ShouldReturnNullMusicBrainzArtistId()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["The Artist"],
            musicBrainzArtistId: "not-a-guid",
            includeMusicBrainzReleaseArtistId: false);

        // Act
        ArtistMetadataDto result = Id3TagMapper.MapArtist(data, "The Artist");

        // Assert
        Assert.Null(result.MusicBrainzArtistId);
    }

    [Fact]
    public void MapAlbum_WhenTheTagsCarryEveryAlbumField_ShouldMapEveryField()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        Guid releaseGroupId = Guid.NewGuid();
        Guid releaseArtistId = Guid.NewGuid();
        Id3TagDto data = _id3TagDtoFixture.Create(
            album: "The Album",
            language: "eng",
            script: "Latn",
            genres: ["Rock"],
            releaseTypes: ["album", "live"],
            releaseStatus: "official",
            mediaFormat: "CD",
            packaging: "Jewel Case",
            discCount: 2,
            trackCount: 12,
            barcode: "1234567890123",
            catalogNumbers: ["CAT-1", "CAT-2"],
            label: "The Label",
            asin: "B000000001",
            originalReleaseDate: new DateOnly(1975, 11, 21),
            originalReleaseYear: 1975,
            releaseCountry: "US",
            musicBrainzReleaseId: releaseId.ToString(),
            musicBrainzReleaseGroupId: releaseGroupId.ToString(),
            musicBrainzReleaseArtistId: releaseArtistId.ToString(),
            albumArtists: ["The Album Artist"]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal("The Album", result.Title);
        Assert.Null(result.OriginalTitle);
        Assert.Null(result.Description);
        Assert.Null(result.Disambiguation);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(new DateOnly(1975, 11, 21), result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(1975, result.ReleaseInfo.OriginalReleaseYear);
        Assert.Null(result.ReleaseInfo.ReReleaseDate);
        Assert.Null(result.ReleaseInfo.ReReleaseYear);
        Assert.Equal(ReleaseCountry.US, result.ReleaseInfo.ReleaseCountry);
        Assert.Null(result.ReleaseInfo.ReleaseVersion);
        Assert.NotNull(result.Language);
        Assert.Equal("en", result.Language!.LanguageCode);
        Assert.Null(result.OriginalLanguage);
        Assert.Null(result.Tags);
        Assert.Equal(["Rock"], result.Genres!.Select(genre => genre.Name));
        Assert.Equal("Latn", result.Script);
        Assert.Equal([MusicReleaseType.Album, MusicReleaseType.Live], result.ReleaseTypes);
        Assert.Equal(MusicReleaseStatus.Official, result.ReleaseStatus);
        Assert.Equal(MusicMediaFormat.CD, result.MediaFormat);
        Assert.Equal(MusicReleasePackaging.JewelCase, result.Packaging);
        Assert.Equal(2, result.TotalDiscs);
        Assert.Equal(12, result.TotalTracks);
        Assert.Equal("1234567890123", result.Barcode);
        Assert.Equal(["CAT-1", "CAT-2"], result.CatalogNumbers);
        Assert.Equal("The Label", result.Label);
        Assert.Equal("B000000001", result.ASIN);
        Assert.Equal(releaseId, result.MusicBrainzReleaseId);
        Assert.Equal(releaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal(releaseArtistId, result.MusicBrainzReleaseArtistId);
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Album Artist", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Performer, contributor.Role);
        Assert.Null(result.Ratings);
        Assert.Null(result.ReleaseTitle);
    }

    [Fact]
    public void MapAlbum_WhenTheTagsAreEmpty_ShouldReturnNullOptionalFields()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            album: "",
            includeLanguage: false,
            includeScript: false,
            genres: [],
            releaseTypes: [],
            includeReleaseStatus: false,
            includeMediaFormat: false,
            includePackaging: false,
            discCount: 0,
            trackCount: 0,
            includeBarcode: false,
            catalogNumbers: [],
            includeLabel: false,
            includeAsin: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReleaseCountry: false,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false,
            includeMusicBrainzReleaseArtistId: false,
            albumArtists: [],
            trackArtists: []);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.Title);
        Assert.Null(result.ReleaseInfo);
        Assert.Null(result.Language);
        Assert.Null(result.Genres);
        Assert.Null(result.Script);
        Assert.Null(result.ReleaseTypes);
        Assert.Null(result.ReleaseStatus);
        Assert.Null(result.MediaFormat);
        Assert.Null(result.Packaging);
        Assert.Null(result.TotalDiscs);
        Assert.Null(result.TotalTracks);
        Assert.Null(result.Barcode);
        Assert.Null(result.CatalogNumbers);
        Assert.Null(result.Label);
        Assert.Null(result.ASIN);
        Assert.Null(result.MusicBrainzReleaseId);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Null(result.MusicBrainzReleaseArtistId);
        Assert.Null(result.Contributors);
    }

    [Fact]
    public void MapAlbum_WhenGenresAreDuplicated_ShouldKeepOnlyTheDistinctOnes()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(genres: ["Rock", "rock", "ROCK"]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal(["Rock"], result.Genres!.Select(genre => genre.Name));
    }

    [Fact]
    public void MapAlbum_WhenThereIsNoReleaseInformation_ShouldReturnNullReleaseInfo()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReleaseCountry: false);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.ReleaseInfo);
    }

    [Fact]
    public void MapAlbum_WhenOnlyTheReleaseCountryIsPresent_ShouldBuildTheReleaseInfoFromIt()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            releaseCountry: "US");

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(ReleaseCountry.US, result.ReleaseInfo!.ReleaseCountry);
        Assert.Null(result.ReleaseInfo.OriginalReleaseDate);
        Assert.Null(result.ReleaseInfo.OriginalReleaseYear);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseCountryIsUnknown_ShouldReturnNullReleaseCountry()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            releaseCountry: "not-a-country");

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.ReleaseInfo);
    }

    [Theory]
    [InlineData("eng", "en", "English")] // a three letter code maps to the neutral culture
    [InlineData("deu", "de", "German")] // another three letter code maps to the neutral culture
    [InlineData("en", "en", "English")] // a two letter code maps too
    [InlineData("zxx", "zxx", "No linguistic content")] // the special code of MusicBrainz
    [InlineData("mul", "mul", "Multiple languages")] // the special code of MusicBrainz
    [InlineData("und", "und", "Undetermined")] // the special code of MusicBrainz
    [InlineData("eng/ara", "en", "English")] // only the first code of a multi language tag is used
    public void MapAlbum_WhenTheLanguageCodeIsPresent_ShouldMapIt(string language, string expectedCode, string expectedName)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(language: language);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.NotNull(result.Language);
        Assert.Equal(expectedCode, result.Language!.LanguageCode);
        Assert.Equal(expectedName, result.Language.LanguageName);
    }

    [Fact]
    public void MapAlbum_WhenTheLanguageCodeIsUnknown_ShouldFallBackToTheRawCode()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(language: "qqq");

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.NotNull(result.Language);
        Assert.Equal("qqq", result.Language!.LanguageCode);
        Assert.Equal("qqq", result.Language.LanguageName);
        Assert.Null(result.Language.NativeName);
    }

    [Theory]
    [InlineData("official", MusicReleaseStatus.Official)]
    [InlineData("promotion", MusicReleaseStatus.Promotion)]
    [InlineData("bootleg", MusicReleaseStatus.Bootleg)]
    [InlineData("pseudo-release", MusicReleaseStatus.PseudoRelease)]
    [InlineData("withdrawn", MusicReleaseStatus.Withdrawn)]
    [InlineData("cancelled", MusicReleaseStatus.Cancelled)]
    [InlineData("expunged", MusicReleaseStatus.Expunged)]
    public void MapAlbum_WhenTheReleaseStatusIsKnown_ShouldMapIt(string status, MusicReleaseStatus expected)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(releaseStatus: status);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal(expected, result.ReleaseStatus);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("made-up")]
    public void MapAlbum_WhenTheReleaseStatusIsUnknown_ShouldReturnNullStatus(string? status)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(releaseStatus: status);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.ReleaseStatus);
    }

    [Theory]
    [InlineData("album", MusicReleaseType.Album)]
    [InlineData("single", MusicReleaseType.Single)]
    [InlineData("ep", MusicReleaseType.Ep)]
    [InlineData("broadcast", MusicReleaseType.Broadcast)]
    [InlineData("other", MusicReleaseType.Other)]
    [InlineData("compilation", MusicReleaseType.Compilation)]
    [InlineData("live", MusicReleaseType.Live)]
    [InlineData("soundtrack", MusicReleaseType.Soundtrack)]
    [InlineData("spokenword", MusicReleaseType.SpokenWord)]
    [InlineData("audio drama", MusicReleaseType.AudioDrama)]
    [InlineData("audiobook", MusicReleaseType.AudioBook)]
    [InlineData("demo", MusicReleaseType.Demo)]
    [InlineData("dj-mix", MusicReleaseType.DjMix)]
    [InlineData("field recording", MusicReleaseType.FieldRecording)]
    [InlineData("interview", MusicReleaseType.Interview)]
    [InlineData("mixtape/street", MusicReleaseType.Mixtape)]
    [InlineData("remix", MusicReleaseType.Remix)]
    public void MapAlbum_WhenTheReleaseTypeIsKnown_ShouldMapIt(string releaseType, MusicReleaseType expected)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(releaseTypes: [releaseType]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal([expected], result.ReleaseTypes);
    }

    [Fact]
    public void MapAlbum_WhenSomeReleaseTypesAreUnknown_ShouldKeepOnlyTheKnownOnesWithoutDuplicates()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(releaseTypes: ["made-up", "album", "Album"]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal([MusicReleaseType.Album], result.ReleaseTypes);
    }

    [Fact]
    public void MapAlbum_WhenEveryReleaseTypeIsEmptyOrUnknown_ShouldReturnNullReleaseTypes()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(releaseTypes: ["", "   ", "made-up"]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.ReleaseTypes);
    }

    [Theory]
    [InlineData("12\" vinyl", MusicMediaFormat.Inch12Vinyl)]
    [InlineData("10\" vinyl", MusicMediaFormat.Inch10Vinyl)]
    [InlineData("7\" vinyl", MusicMediaFormat.Inch7Vinyl)]
    [InlineData("3\" vinyl", MusicMediaFormat.Inch3Vinyl)]
    [InlineData("vinyl", MusicMediaFormat.Vinyl)]
    [InlineData("cd", MusicMediaFormat.CD)]
    [InlineData("sacd", MusicMediaFormat.SACD)]
    [InlineData("hybrid sacd", MusicMediaFormat.HybridSACD)]
    [InlineData("digital media", MusicMediaFormat.DigitalMedia)]
    [InlineData("cassette", MusicMediaFormat.Cassette)]
    [InlineData("dvd", MusicMediaFormat.DVD)]
    [InlineData("dvd-video", MusicMediaFormat.DVDVideo)]
    [InlineData("dvd-audio", MusicMediaFormat.DVDAudio)]
    [InlineData("blu-ray", MusicMediaFormat.BluRay)]
    [InlineData("minidisc", MusicMediaFormat.MiniDisc)]
    [InlineData("8cm cd", MusicMediaFormat.Cm8CD)]
    [InlineData("dat", MusicMediaFormat.DAT)]
    [InlineData("vinyl disc", MusicMediaFormat.VinylDisc)] // parsed through the normalized fallback
    [InlineData("other", MusicMediaFormat.Other)]
    public void MapAlbum_WhenTheMediaFormatIsKnown_ShouldMapIt(string mediaFormat, MusicMediaFormat expected)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(mediaFormat: mediaFormat);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal(expected, result.MediaFormat);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("made-up")]
    public void MapAlbum_WhenTheMediaFormatIsUnknown_ShouldReturnNullMediaFormat(string? mediaFormat)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(mediaFormat: mediaFormat);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.MediaFormat);
    }

    [Theory]
    [InlineData("book", MusicReleasePackaging.Book)]
    [InlineData("box", MusicReleasePackaging.Box)]
    [InlineData("cardboard/paper sleeve", MusicReleasePackaging.CardboardPaperSleeve)]
    [InlineData("jewel case", MusicReleasePackaging.JewelCase)]
    [InlineData("digipak", MusicReleasePackaging.Digipak)]
    [InlineData("metal tin", MusicReleasePackaging.MetalTin)]
    [InlineData("none", MusicReleasePackaging.None)]
    [InlineData("other", MusicReleasePackaging.Other)]
    public void MapAlbum_WhenThePackagingIsKnown_ShouldMapIt(string packaging, MusicReleasePackaging expected)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(packaging: packaging);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal(expected, result.Packaging);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("made-up")]
    public void MapAlbum_WhenThePackagingIsUnknown_ShouldReturnNullPackaging(string? packaging)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(packaging: packaging);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.Packaging);
    }

    [Fact]
    public void MapAlbum_WhenAlbumArtistsArePresent_ShouldMapThemAsPerformerContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            albumArtists: ["Album Artist One", "Album Artist Two"],
            trackArtists: ["Track Artist"]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Equal(2, result.Contributors!.Count);
        Assert.All(result.Contributors, contributor => Assert.Equal(MediaContributorRole.Performer, contributor.Role));
        Assert.Equal(["Album Artist One", "Album Artist Two"], result.Contributors.Select(contributor => contributor.Name!.DisplayName));
    }

    [Fact]
    public void MapAlbum_WhenThereAreNoAlbumArtists_ShouldFallBackToTheTrackArtistsForTheContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(albumArtists: [], trackArtists: ["Track Artist"]);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("Track Artist", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Performer, contributor.Role);
    }

    [Fact]
    public void MapAlbum_WhenThereAreNoArtistsAtAll_ShouldReturnNullContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(albumArtists: [], trackArtists: []);

        // Act
        AlbumMetadataDto result = Id3TagMapper.MapAlbum(data);

        // Assert
        Assert.Null(result.Contributors);
    }

    [Fact]
    public void MapTrack_WhenTheTagsCarryEveryTrackField_ShouldMapEveryField()
    {
        // Arrange
        Guid recordingId = Guid.NewGuid();
        Guid releaseTrackId = Guid.NewGuid();
        Guid workId = Guid.NewGuid();
        Id3TagDto data = _id3TagDtoFixture.Create(
            title: "The Track",
            language: "eng",
            script: "Latn",
            genres: ["Rock"],
            musicKey: "C#m",
            bpm: 128,
            isrcs: ["US1A2B3C4D5E"],
            moods: ["calm"],
            durationInSeconds: 215,
            sampleRate: 44100,
            channels: 2,
            bitDepth: 24,
            bitrate: 320,
            audioCodec: "MPEG Audio",
            originalReleaseDate: new DateOnly(1975, 11, 21),
            originalReleaseYear: 1975,
            releaseCountry: "US",
            musicBrainzRecordingId: recordingId.ToString(),
            musicBrainzReleaseTrackId: releaseTrackId.ToString(),
            musicBrainzWorkId: workId.ToString(),
            workTitle: "The Work",
            trackArtists: ["The Artist"],
            composers: ["The Composer"],
            lyricists: ["The Lyricist"],
            conductors: ["The Conductor"],
            remixers: ["The Remixer"],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Equal("The Track", result.Title);
        Assert.Null(result.OriginalTitle);
        Assert.Null(result.Description);
        Assert.Null(result.Disambiguation);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(new DateOnly(1975, 11, 21), result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(1975, result.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(ReleaseCountry.US, result.ReleaseInfo.ReleaseCountry);
        Assert.NotNull(result.Language);
        Assert.Equal("en", result.Language!.LanguageCode);
        Assert.Null(result.OriginalLanguage);
        Assert.Null(result.Tags);
        Assert.Equal(["Rock"], result.Genres!.Select(genre => genre.Name));
        Assert.Equal("Latn", result.Script);
        Assert.Equal(MusicKey.CSharpMinor, result.Key);
        Assert.Equal(128, result.Bpm);
        Assert.False(result.IsVideo);
        Assert.NotNull(result.Work);
        Assert.Equal(workId, result.Work!.MusicBrainzWorkId);
        Assert.Equal("The Work", result.Work.Title);
        Assert.Equal("US1A2B3C4D5E", Assert.Single(result.Isrcs!).Value);
        Assert.Equal("calm", Assert.Single(result.Moods!).Name);
        Assert.Equal(215, result.DurationInSeconds);
        Assert.Equal(44100, result.SampleRate);
        Assert.Equal(2, result.Channels);
        Assert.Equal(24, result.BitDepth);
        Assert.Equal(320, result.Bitrate);
        Assert.Equal("MPEG Audio", result.AudioCodec);
        Assert.Equal(recordingId, result.MusicBrainzRecordingId);
        Assert.Equal(releaseTrackId, result.MusicBrainzTrackId);
        Assert.Null(result.Ratings);
        Assert.Equal(5, result.Contributors!.Count);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Artist" && contributor.Role == MediaContributorRole.Performer);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Composer" && contributor.Role == MediaContributorRole.Composer);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Lyricist" && contributor.Role == MediaContributorRole.Lyricist);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Conductor" && contributor.Role == MediaContributorRole.Conductor);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Remixer" && contributor.Role == MediaContributorRole.Remixer);
    }

    [Fact]
    public void MapTrack_WhenTheTagsAreEmpty_ShouldReturnNullOptionalFields()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            title: "",
            includeLanguage: false,
            includeScript: false,
            genres: [],
            includeMusicKey: false,
            includeBpm: false,
            isrcs: [],
            moods: [],
            includeDurationInSeconds: false,
            includeSampleRate: false,
            includeChannels: false,
            includeBitDepth: false,
            includeBitrate: false,
            includeAudioCodec: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReleaseCountry: false,
            includeMusicBrainzRecordingId: false,
            includeMusicBrainzReleaseTrackId: false,
            includeMusicBrainzWorkId: false,
            includeWorkTitle: false,
            trackArtists: [],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Title);
        Assert.Null(result.ReleaseInfo);
        Assert.Null(result.Language);
        Assert.Null(result.Genres);
        Assert.Null(result.Script);
        Assert.Null(result.Key);
        Assert.Null(result.Bpm);
        Assert.Null(result.Work);
        Assert.Null(result.Isrcs);
        Assert.Null(result.Moods);
        Assert.Null(result.DurationInSeconds);
        Assert.Null(result.SampleRate);
        Assert.Null(result.Channels);
        Assert.Null(result.BitDepth);
        Assert.Null(result.Bitrate);
        Assert.Null(result.AudioCodec);
        Assert.Null(result.MusicBrainzRecordingId);
        Assert.Null(result.MusicBrainzTrackId);
        Assert.Null(result.Contributors);
    }

    [Fact]
    public void MapTrack_WhenThereIsNoReleaseInformation_ShouldReturnNullReleaseInfo()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReleaseCountry: false);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.ReleaseInfo);
    }

    [Fact]
    public void MapTrack_WhenOnlyTheReleaseCountryIsPresent_ShouldBuildTheReleaseInfoFromIt()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            releaseCountry: "US");

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(ReleaseCountry.US, result.ReleaseInfo!.ReleaseCountry);
    }

    [Fact]
    public void MapTrack_WhenBothTheWorkIdAndTheWorkTitleArePresent_ShouldMapTheWork()
    {
        // Arrange
        Guid workId = Guid.NewGuid();
        Id3TagDto data = _id3TagDtoFixture.Create(
            musicBrainzWorkId: workId.ToString(),
            workTitle: "The Work");

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.NotNull(result.Work);
        Assert.Equal(workId, result.Work!.MusicBrainzWorkId);
        Assert.Equal("The Work", result.Work.Title);
        Assert.Null(result.Work.Type);
        Assert.Null(result.Work.Languages);
        Assert.Null(result.Work.Iswcs);
    }

    [Fact]
    public void MapTrack_WhenTheWorkIdIsSlashSeparated_ShouldUseTheFirstParsableIdentifier()
    {
        // Arrange
        Guid workId = Guid.NewGuid();
        Id3TagDto data = _id3TagDtoFixture.Create(
            musicBrainzWorkId: $"not-a-guid/{workId}",
            workTitle: "The Work");

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.NotNull(result.Work);
        Assert.Equal(workId, result.Work!.MusicBrainzWorkId);
    }

    [Fact]
    public void MapTrack_WhenTheWorkTitleIsSlashSeparated_ShouldUseTheFirstValue()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            musicBrainzWorkId: Guid.NewGuid().ToString(),
            workTitle: "The Work/Alternate");

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.NotNull(result.Work);
        Assert.Equal("The Work", result.Work!.Title);
    }

    [Fact]
    public void MapTrack_WhenOnlyTheWorkIdIsPresent_ShouldReturnNullWork()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            musicBrainzWorkId: Guid.NewGuid().ToString(),
            includeWorkTitle: false);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Work);
    }

    [Fact]
    public void MapTrack_WhenOnlyTheWorkTitleIsPresent_ShouldReturnNullWork()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            includeMusicBrainzWorkId: false,
            workTitle: "The Work");

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Work);
    }

    [Fact]
    public void MapTrack_WhenTheWorkIdCannotBeParsed_ShouldReturnNullWork()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            musicBrainzWorkId: "not-a-guid/also-not-a-guid",
            workTitle: "The Work");

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Work);
    }

    [Theory]
    [InlineData("C", MusicKey.CMajor)]
    [InlineData("C major", MusicKey.CMajor)]
    [InlineData("C maj", MusicKey.CMajor)]
    [InlineData("Cm", MusicKey.CMinor)]
    [InlineData("C min", MusicKey.CMinor)]
    [InlineData("C#m", MusicKey.CSharpMinor)]
    [InlineData("C# minor", MusicKey.CSharpMinor)]
    [InlineData("Db", MusicKey.CSharpMajor)]
    [InlineData("D", MusicKey.DMajor)]
    [InlineData("Dm", MusicKey.DMinor)]
    [InlineData("D# minor", MusicKey.DSharpMinor)]
    [InlineData("E", MusicKey.EMajor)]
    [InlineData("Em", MusicKey.EMinor)]
    [InlineData("F", MusicKey.FMajor)]
    [InlineData("Fm", MusicKey.FMinor)]
    [InlineData("G", MusicKey.GMajor)]
    [InlineData("Gm", MusicKey.GMinor)]
    [InlineData("E\u266d", MusicKey.DSharpMajor)] // the flat sign is normalized to the sharp equivalent
    [InlineData("F# major", MusicKey.FSharpMajor)]
    [InlineData("Gb", MusicKey.FSharpMajor)]
    [InlineData("G#m", MusicKey.GSharpMinor)]
    [InlineData("Ab", MusicKey.GSharpMajor)]
    [InlineData("A#m", MusicKey.ASharpMinor)]
    [InlineData("Bb", MusicKey.ASharpMajor)]
    [InlineData("am", MusicKey.AMinor)]
    [InlineData("b", MusicKey.BMajor)]
    public void MapTrack_WhenTheMusicalKeyIsKnown_ShouldMapIt(string musicKey, MusicKey expected)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(musicKey: musicKey);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Equal(expected, result.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("H")]
    [InlineData("Hm")]
    public void MapTrack_WhenTheMusicalKeyIsUnknown_ShouldReturnNullKey(string? musicKey)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(musicKey: musicKey);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Key);
    }

    [Fact]
    public void MapTrack_WhenTheTrackArtistsArePresent_ShouldMapThemAsPerformerContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["Track Artist"],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("Track Artist", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Performer, contributor.Role);
    }

    [Fact]
    public void MapTrack_WhenTheComposersArePresent_ShouldMapThemAsComposerContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: ["The Composer"],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Composer", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Composer, contributor.Role);
    }

    [Fact]
    public void MapTrack_WhenTheLyricistsArePresent_ShouldMapThemAsLyricistContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: [],
            lyricists: ["The Lyricist"],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Lyricist", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Lyricist, contributor.Role);
    }

    [Fact]
    public void MapTrack_WhenTheConductorsArePresent_ShouldMapThemAsConductorContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: [],
            lyricists: [],
            conductors: ["The Conductor"],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Conductor", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Conductor, contributor.Role);
    }

    [Fact]
    public void MapTrack_WhenTheRemixersArePresent_ShouldMapThemAsRemixerContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: ["The Remixer"],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Remixer", contributor.Name!.DisplayName);
        Assert.Equal(MediaContributorRole.Remixer, contributor.Role);
    }

    [Theory]
    [InlineData("producer", MediaContributorRole.Producer)]
    [InlineData("executive producer", MediaContributorRole.ExecutiveProducer)]
    [InlineData("backing vocals", MediaContributorRole.BackingVocals)]
    [InlineData("vocals", MediaContributorRole.Vocals)]
    [InlineData("electric bass", MediaContributorRole.BassGuitar)]
    [InlineData("bass", MediaContributorRole.BassGuitar)]
    [InlineData("guitar", MediaContributorRole.Guitar)]
    [InlineData("drums", MediaContributorRole.Drums)]
    [InlineData("membranophone", MediaContributorRole.Drums)]
    [InlineData("percussion", MediaContributorRole.Percussion)]
    [InlineData("tambourine", MediaContributorRole.Percussion)]
    [InlineData("keyboard", MediaContributorRole.Keyboards)]
    [InlineData("piano", MediaContributorRole.Piano)]
    [InlineData("synthesizer", MediaContributorRole.Synthesizer)]
    [InlineData("sampler", MediaContributorRole.Synthesizer)]
    [InlineData("violin", MediaContributorRole.Strings)]
    [InlineData("trumpet", MediaContributorRole.Brass)]
    [InlineData("flute", MediaContributorRole.Woodwinds)]
    [InlineData("remix", MediaContributorRole.Remixer)]
    [InlineData("mastering", MediaContributorRole.Engineer)]
    [InlineData("mix", MediaContributorRole.Mixer)]
    [InlineData("arranger", MediaContributorRole.Arranger)]
    [InlineData("conductor", MediaContributorRole.Conductor)]
    [InlineData("orchestrator", MediaContributorRole.Orchestrator)]
    [InlineData("composer", MediaContributorRole.Composer)]
    [InlineData("librettist", MediaContributorRole.Lyricist)]
    [InlineData("author", MediaContributorRole.Author)]
    [InlineData("chorus", MediaContributorRole.Choir)]
    [InlineData("member of band", MediaContributorRole.Performer)]
    [InlineData("orchestra", MediaContributorRole.Performer)]
    [InlineData("made-up role", MediaContributorRole.Other)]
    public void MapTrack_WhenInvolvedPeopleArePresent_ShouldMapTheirRoles(string role, MediaContributorRole expected)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: [(role, "The Person")]);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Person", contributor.Name!.DisplayName);
        Assert.Equal(expected, contributor.Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MapTrack_WhenAnInvolvedPersonHasNoRole_ShouldMapThemAsOther(string? role)
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: [(role!, "The Person")]);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal(MediaContributorRole.Other, contributor.Role);
    }

    [Fact]
    public void MapTrack_WhenTheSameContributorIsCreditedTwiceWithTheSameRole_ShouldKeepASingleContributor()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["The Artist", "The Artist"],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Single(result.Contributors!);
    }

    [Fact]
    public void MapTrack_WhenTheSameNameIsCreditedWithTwoRoles_ShouldKeepBothContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["The Person"],
            composers: ["The Person"],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Equal(2, result.Contributors!.Count);
    }

    [Fact]
    public void MapTrack_WhenAContributorNameIsWhiteSpace_ShouldSkipIt()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: ["", "   "],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Contributors);
    }

    [Fact]
    public void MapTrack_WhenThereAreNoContributors_ShouldReturnNullContributors()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(
            trackArtists: [],
            composers: [],
            lyricists: [],
            conductors: [],
            remixers: [],
            involvedPeople: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Contributors);
    }

    [Fact]
    public void MapTrack_WhenTheIsrcsAreDuplicated_ShouldKeepOnlyTheDistinctOnes()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(isrcs: ["US1A2B3C4D5E", "us1a2b3c4d5e"]);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Single(result.Isrcs!);
    }

    [Fact]
    public void MapTrack_WhenTheMoodsAreDuplicated_ShouldKeepOnlyTheDistinctOnes()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(moods: ["calm", "CALM"]);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Single(result.Moods!);
    }

    [Fact]
    public void MapTrack_WhenTheMoodsAreAbsent_ShouldReturnNullMoods()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(moods: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Moods);
    }

    [Fact]
    public void MapTrack_WhenTheGenresAreAbsent_ShouldReturnNullGenres()
    {
        // Arrange
        Id3TagDto data = _id3TagDtoFixture.Create(genres: []);

        // Act
        AudioMetadataDto result = Id3TagMapper.MapTrack(data);

        // Assert
        Assert.Null(result.Genres);
    }
}
