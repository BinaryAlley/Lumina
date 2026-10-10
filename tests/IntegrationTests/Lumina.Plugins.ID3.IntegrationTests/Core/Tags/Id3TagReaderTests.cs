#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using Lumina.Plugins.ID3.Core.Tags;
using Lumina.Plugins.ID3.Fixtures.Core.Tags;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Plugins.ID3.IntegrationTests.Core.Tags;

/// <summary>
/// Contains integration tests for the <see cref="Id3TagReader"/> class, reading real audio containers through the tagging library.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3TagReaderTests : IDisposable
{
    private readonly Id3TagReader _sut = new();
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Id3TagReaderTests"/> class.
    /// </summary>
    public Id3TagReaderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-id3-reader-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public void Read_WhenEveryTagIsPresent_ShouldMapEveryProperty()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "all-fields.wav");
        Guid artistId = Guid.NewGuid();
        Guid releaseArtistId = Guid.NewGuid();
        Guid releaseGroupId = Guid.NewGuid();
        Guid releaseId = Guid.NewGuid();
        Guid recordingId = Guid.NewGuid();
        Guid releaseTrackId = Guid.NewGuid();
        Guid workId = Guid.NewGuid();
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            file.Tag.Title = "The Track";
            file.Tag.Album = "The Album";
            file.Tag.Performers = ["Track Artist"];
            file.Tag.AlbumArtists = ["Album Artist"];
            file.Tag.PerformersSort = ["Track Artist, The"];
            file.Tag.AlbumArtistsSort = ["Album Artist, The"];
            file.Tag.Track = 3;
            file.Tag.TrackCount = 12;
            file.Tag.Disc = 1;
            file.Tag.DiscCount = 2;
            file.Tag.Genres = ["Rock/Pop", "Jazz"];
            file.Tag.Composers = ["Composer One/Composer Two"];
            file.Tag.Conductor = "The Conductor";
            file.Tag.RemixedBy = "The Remixer";
            file.Tag.Publisher = "The Label";
            file.Tag.ISRC = "GBAAA0000001";
            file.Tag.InitialKey = "C#m";
            file.Tag.BeatsPerMinute = 128;
            file.Tag.AmazonId = "B000000001";
            file.Tag.MusicBrainzArtistId = artistId.ToString();
            file.Tag.MusicBrainzReleaseArtistId = releaseArtistId.ToString();
            file.Tag.MusicBrainzReleaseGroupId = releaseGroupId.ToString();
            file.Tag.MusicBrainzReleaseId = releaseId.ToString();
            file.Tag.MusicBrainzTrackId = recordingId.ToString();
            file.Tag.MusicBrainzReleaseType = "album";
            file.Tag.MusicBrainzReleaseStatus = "official";
            file.Tag.MusicBrainzReleaseCountry = "US";
            TestAudioFileFactory.AddTextFrame(file, "TDRC", "2011-03-14");
            TestAudioFileFactory.AddTextFrame(file, "TDOR", "1975-11-21");
            TestAudioFileFactory.AddUserTextFrame(file, "LYRICIST", "Lyricist One;Lyricist Two");
            TestAudioFileFactory.AddTextFrame(file, "IPLS", "producer", "Producer Person", "engineer", "Engineer Person");
            TestAudioFileFactory.AddUserTextFrame(file, "LANGUAGE", "eng");
            TestAudioFileFactory.AddUserTextFrame(file, "SCRIPT", "Latn");
            TestAudioFileFactory.AddUserTextFrame(file, "CATALOGNUMBER", "CAT-1;CAT-2");
            TestAudioFileFactory.AddUserTextFrame(file, "BARCODE", "1234567890123");
            TestAudioFileFactory.AddUserTextFrame(file, "MEDIA", "CD");
            TestAudioFileFactory.AddUserTextFrame(file, "PACKAGING", "Jewel Case");
            TestAudioFileFactory.AddUserTextFrame(file, "MOODS", "calm;happy");
            TestAudioFileFactory.AddUserTextFrame(file, "WORK", "The Work");
            TestAudioFileFactory.AddUserTextFrame(file, "MUSICBRAINZ_WORKID", workId.ToString());
            TestAudioFileFactory.AddUserTextFrame(file, "MUSICBRAINZ_RELEASETRACKID", releaseTrackId.ToString());
            TestAudioFileFactory.AddUserTextFrame(file, "website", "https://artist.example");
        }, durationMs: 2000);

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Track", result.Title);
        Assert.Equal("The Album", result.Album);
        Assert.Equal(["Track Artist"], result.TrackArtists);
        Assert.Equal(["Album Artist"], result.AlbumArtists);
        Assert.Equal("Album Artist, The", result.AlbumArtistSortName);
        Assert.Equal("Track Artist, The", result.TrackArtistSortName);
        Assert.Equal(3u, result.TrackNumber);
        Assert.Equal(12u, result.TrackCount);
        Assert.Equal(1u, result.DiscNumber);
        Assert.Equal(2u, result.DiscCount);
        Assert.Equal(2011, result.Year);
        Assert.Equal(new DateOnly(1975, 11, 21), result.OriginalReleaseDate);
        Assert.Equal(1975, result.OriginalReleaseYear);
        Assert.Equal(["Rock", "Pop", "Jazz"], result.Genres);
        Assert.Equal(["Composer One", "Composer Two"], result.Composers);
        Assert.Equal(["Lyricist One", "Lyricist Two"], result.Lyricists);
        Assert.Equal(["The Conductor"], result.Conductors);
        Assert.Equal(["The Remixer"], result.Remixers);
        Assert.Equal([("producer", "Producer Person"), ("engineer", "Engineer Person")], result.InvolvedPeople);
        Assert.Equal("eng", result.Language);
        Assert.Equal("Latn", result.Script);
        Assert.Equal("The Label", result.Label);
        Assert.Equal(["CAT-1", "CAT-2"], result.CatalogNumbers);
        Assert.Equal("1234567890123", result.Barcode);
        Assert.Equal(["album"], result.ReleaseTypes);
        Assert.Equal("official", result.ReleaseStatus);
        Assert.Equal("US", result.ReleaseCountry);
        Assert.Equal("CD", result.MediaFormat);
        Assert.Equal("Jewel Case", result.Packaging);
        Assert.Equal("B000000001", result.Asin);
        Assert.Equal(["GBAAA0000001"], result.Isrcs);
        Assert.Equal(["calm", "happy"], result.Moods);
        Assert.Equal("The Work", result.WorkTitle);
        Assert.Equal("C#m", result.MusicKey);
        Assert.Equal(128, result.Bpm);
        Assert.Equal(2, result.DurationInSeconds);
        Assert.Equal(44100, result.SampleRate);
        Assert.Equal(1, result.Channels);
        Assert.Equal(16, result.BitDepth);
        Assert.True(result.Bitrate > 0);
        Assert.Equal("PCM Audio", result.AudioCodec);
        Assert.Equal("https://artist.example", result.Website);
        Assert.Equal(artistId.ToString(), result.MusicBrainzArtistId);
        Assert.Equal(releaseArtistId.ToString(), result.MusicBrainzReleaseArtistId);
        Assert.Equal(releaseGroupId.ToString(), result.MusicBrainzReleaseGroupId);
        Assert.Equal(releaseId.ToString(), result.MusicBrainzReleaseId);
        Assert.Equal(recordingId.ToString(), result.MusicBrainzRecordingId);
        Assert.Equal(releaseTrackId.ToString(), result.MusicBrainzReleaseTrackId);
        Assert.Equal(workId.ToString(), result.MusicBrainzWorkId);
    }

    [Fact]
    public void Read_WhenTheFileIsUntagged_ShouldReturnASnapshotWithTheAudioProperties()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "untagged.wav");
        TestAudioFileFactory.CreateWav(path);

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Title);
        Assert.Null(result.Album);
        Assert.Empty(result.TrackArtists);
        Assert.Empty(result.AlbumArtists);
        Assert.Null(result.AlbumArtistSortName);
        Assert.Null(result.TrackArtistSortName);
        Assert.Equal(0u, result.TrackNumber);
        Assert.Equal(0u, result.TrackCount);
        Assert.Equal(0u, result.DiscNumber);
        Assert.Equal(0u, result.DiscCount);
        Assert.Null(result.Year);
        Assert.Null(result.OriginalReleaseDate);
        Assert.Null(result.OriginalReleaseYear);
        Assert.Empty(result.Genres);
        Assert.Empty(result.Composers);
        Assert.Empty(result.Lyricists);
        Assert.Empty(result.Conductors);
        Assert.Empty(result.Remixers);
        Assert.Empty(result.InvolvedPeople);
        Assert.Null(result.Language);
        Assert.Null(result.Script);
        Assert.Null(result.Label);
        Assert.Empty(result.CatalogNumbers);
        Assert.Null(result.Barcode);
        Assert.Empty(result.ReleaseTypes);
        Assert.Null(result.ReleaseStatus);
        Assert.Null(result.ReleaseCountry);
        Assert.Null(result.MediaFormat);
        Assert.Null(result.Packaging);
        Assert.Null(result.Asin);
        Assert.Empty(result.Isrcs);
        Assert.Empty(result.Moods);
        Assert.Null(result.WorkTitle);
        Assert.Null(result.MusicKey);
        Assert.Null(result.Bpm);
        Assert.Equal(44100, result.SampleRate);
        Assert.Equal(1, result.Channels);
        Assert.Equal(16, result.BitDepth);
        Assert.Equal("PCM Audio", result.AudioCodec);
        Assert.Null(result.Website);
        Assert.Null(result.MusicBrainzArtistId);
        Assert.Null(result.MusicBrainzReleaseArtistId);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Null(result.MusicBrainzReleaseId);
        Assert.Null(result.MusicBrainzRecordingId);
        Assert.Null(result.MusicBrainzReleaseTrackId);
        Assert.Null(result.MusicBrainzWorkId);
    }

    [Fact]
    public void Read_WhenTheFileIsNotAnAudioContainer_ShouldReturnNull()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "not-audio.txt");
        File.WriteAllText(path, "this is not an audio file");

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Read_WhenCalledTwiceForTheSamePath_ShouldReturnTheCachedSnapshot()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "cached.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => file.Tag.Title = "The Track");

        // Act
        Id3TagDto? first = _sut.Read(path);
        Id3TagDto? second = _sut.Read(path);

        // Assert
        Assert.NotNull(first);
        Assert.Same(first, second);
    }

    [Theory]
    [InlineData("2011-03-14", 2011)] // a full date yields its year
    [InlineData("2011", 2011)] // a bare year is matched by the year pattern
    [InlineData("released in 1999 remastered", 1999)] // a year embedded in free form text is still matched
    public void Read_WhenTheReleaseDateIsPresent_ShouldParseTheYear(string releaseDate, int expectedYear)
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, $"release-date-{Guid.NewGuid():N}.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => TestAudioFileFactory.AddUserTextFrame(file, "TDRC", releaseDate));

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedYear, result.Year);
    }

    [Fact]
    public void Read_WhenOnlyTheYearTagIsPresent_ShouldMapTheYear()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "year-only.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => file.Tag.Year = 1999);

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1999, result.Year);
        Assert.Null(result.OriginalReleaseDate);
        Assert.Equal(1999, result.OriginalReleaseYear);
    }

    [Fact]
    public void Read_WhenOnlyTheOriginalYearIsPresent_ShouldUseItForTheOriginalReleaseYear()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "original-year.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            file.Tag.Year = 2000;
            TestAudioFileFactory.AddUserTextFrame(file, "originalyear", "1960");
        });

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000, result.Year);
        Assert.Null(result.OriginalReleaseDate);
        Assert.Equal(1960, result.OriginalReleaseYear);
    }

    [Fact]
    public void Read_WhenOnlyTheOriginalReleaseDateIsPresent_ShouldDeriveTheOriginalReleaseYearFromIt()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "original-date.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            TestAudioFileFactory.AddUserTextFrame(file, "TDRC", "2011-03-14");
            TestAudioFileFactory.AddUserTextFrame(file, "TDOR", "1975-11-21");
        });

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(1975, 11, 21), result.OriginalReleaseDate);
        Assert.Equal(1975, result.OriginalReleaseYear);
    }

    [Fact]
    public void Read_WhenTheLabelIsOnlyCarriedByTheExtendedTag_ShouldUseIt()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "extended-label.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => TestAudioFileFactory.AddUserTextFrame(file, "LABEL", "The Extended Label"));

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Extended Label", result.Label);
    }

    [Fact]
    public void Read_WhenTheAsinIsOnlyCarriedByTheExtendedTag_ShouldUseIt()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "extended-asin.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => TestAudioFileFactory.AddUserTextFrame(file, "ASIN", "B000000001"));

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("B000000001", result.Asin);
    }

    [Fact]
    public void Read_WhenTheLanguageUsesTheAlternativeKey_ShouldStillMapIt()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tlan.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => TestAudioFileFactory.AddUserTextFrame(file, "TLAN", "deu"));

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("deu", result.Language);
    }

    [Fact]
    public void Read_WhenTheIsrcIsBothInTheDedicatedAndTheExtendedTag_ShouldKeepOnlyDistinctValues()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "isrc-merge.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            file.Tag.ISRC = "GBAAA0000001";
            TestAudioFileFactory.AddTextFrame(file, "TSRC", "GBAAA0000001", "USRC17607839");
        });

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(["GBAAA0000001", "USRC17607839"], result.Isrcs);
    }

    [Fact]
    public void Read_WhenSeveralReleaseTypesArePacked_ShouldSplitThemOnSemicolons()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "release-types.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => file.Tag.MusicBrainzReleaseType = "album;live");

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(["album", "live"], result.ReleaseTypes);
    }

    [Fact]
    public void Read_WhenTheWebsiteIsAUrlLinkFrame_ShouldMapIt()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "woar.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => TestAudioFileFactory.AddUrlFrame(file, "WOAR", "https://artist.example"));

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://artist.example", result.Website);
    }

    [Fact]
    public void Read_WhenAFrameCarriesNoDescription_ShouldIgnoreIt()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "no-description.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            TestAudioFileFactory.AddUserTextFrame(file, "", "orphan");
            file.Tag.Title = "The Track";
        });

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Track", result.Title);
    }

    [Fact]
    public void Read_WhenAppleStyleInvolvedPeopleKeysArePresent_ShouldMapThem()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "apple-people.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            TestAudioFileFactory.AddUserTextFrame(file, "performer", "Performer Person");
            TestAudioFileFactory.AddUserTextFrame(file, "PRODUCER", "Producer Person");
            TestAudioFileFactory.AddUserTextFrame(file, "ENGINEER", "Engineer Person");
        });

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([("performer", "Performer Person"), ("producer", "Producer Person"), ("engineer", "Engineer Person")], result.InvolvedPeople);
    }

    [Fact]
    public void Read_WhenMoreThanTheCacheLimitFilesAreRead_ShouldEvictTheOldestSnapshot()
    {
        // Arrange
        string[] paths = new string[513];
        for (int index = 0; index < paths.Length; index++)
        {
            paths[index] = Path.Combine(_temporaryDirectory, $"cache-{index}.wav");
            TestAudioFileFactory.CreateWav(paths[index], durationMs: 1);
        }
        Id3TagDto? firstBeforeEviction = _sut.Read(paths[0]);

        // Act
        for (int index = 1; index < paths.Length; index++)
            _sut.Read(paths[index]);
        Id3TagDto? firstAfterEviction = _sut.Read(paths[0]);
        Id3TagDto? last = _sut.Read(paths[^1]);

        // Assert
        Assert.NotNull(firstBeforeEviction);
        Assert.NotNull(firstAfterEviction);
        // The first snapshot was evicted to make room, so reading it again produced a fresh snapshot.
        Assert.NotSame(firstBeforeEviction, firstAfterEviction);
        // The most recently read snapshot is still cached.
        Assert.Same(_sut.Read(paths[^1]), last);
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
        GC.SuppressFinalize(this);
    }
}
