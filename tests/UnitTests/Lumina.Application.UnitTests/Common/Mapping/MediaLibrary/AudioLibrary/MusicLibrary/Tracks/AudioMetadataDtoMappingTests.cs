#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="AudioMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioMetadataDtoMappingTests
{
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();

    [Fact]
    public void ApplyTo_WhenMappingCompleteDto_ShouldApplyAllPropertiesCorrectly()
    {
        // Arrange
        (Artist artist, Album album, Track track) = CreateDomainArtist();
        AudioMetadataDto dto = _audioMetadataDtoFixture.Create();
        List<MusicMediaContributor> contributors = _musicMediaContributorFixture.CreateMany(2);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, track, contributors);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.Title, track.Metadata.Title);
        Assert.Equal(dto.DurationInSeconds, track.Metadata.DurationInSeconds);
        Assert.Equal(dto.SampleRate, track.Metadata.SampleRate);
        Assert.Equal(dto.Channels, track.Metadata.Channels);
        Assert.Equal(dto.BitDepth, track.Metadata.BitDepth.Value);
        Assert.Equal(dto.AudioCodec, track.Metadata.AudioCodec.Value);
        Assert.Equal(dto.Bitrate, track.Metadata.Bitrate.Value);
        Assert.True(track.Script.HasValue);
        Assert.Equal(dto.Script, track.Script.Value);
        Assert.True(track.Key.HasValue);
        Assert.Equal(dto.Key, track.Key.Value);
        Assert.True(track.Bpm.HasValue);
        Assert.Equal(dto.Bpm, track.Bpm.Value);
        Assert.Equal(dto.IsVideo, track.IsVideo);
        Assert.True(track.Work.HasValue);
        Assert.Equal(dto.Work!.MusicBrainzWorkId, track.Work.Value.MusicBrainzWorkId.Value);
        Assert.Equal(dto.Isrcs!.Count, track.Isrcs.Count);
        Assert.Equal(dto.Moods!.Count, track.Moods.Count);
        Assert.True(track.MusicBrainzRecordingId.HasValue);
        Assert.Equal(dto.MusicBrainzRecordingId, track.MusicBrainzRecordingId.Value.Value);
        Assert.True(track.MusicBrainzTrackId.HasValue);
        Assert.Equal(dto.MusicBrainzTrackId, track.MusicBrainzTrackId.Value.Value);
        Assert.Equal(contributors.Count, track.Contributors.Count);
        Assert.Equal(dto.Ratings!.Count, track.Ratings.Count);
    }

    [Fact]
    public void ApplyTo_WhenProviderFieldsAreMissing_ShouldPreserveTheLocallyExtractedValues()
    {
        // Arrange
        (Artist artist, Album album, Track track) = CreateDomainArtist();
        int storedDuration = track.Metadata.DurationInSeconds;
        int storedSampleRate = track.Metadata.SampleRate;
        int storedChannels = track.Metadata.Channels;
        bool hadStoredWork = track.Work.HasValue;
        int storedIsrcCount = track.Isrcs.Count;
        AudioMetadataDto dto = _audioMetadataDtoFixture.Create(
            includeDurationInSeconds: false,
            includeSampleRate: false,
            includeChannels: false,
            includeBitDepth: false,
            includeAudioCodec: false,
            includeBitrate: false,
            includeWork: false,
            includeIsrcs: false,
            includeMoods: false);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, track, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(storedDuration, track.Metadata.DurationInSeconds);
        Assert.Equal(storedSampleRate, track.Metadata.SampleRate);
        Assert.Equal(storedChannels, track.Metadata.Channels);
        Assert.Equal(hadStoredWork, track.Work.HasValue);
        Assert.Equal(storedIsrcCount, track.Isrcs.Count);
    }

    [Fact]
    public void ApplyTo_WhenTrackDoesNotBelongToAlbum_ShouldReturnTrackNotFound()
    {
        // Arrange
        (Artist artist, Album album, Track _) = CreateDomainArtist();
        (Artist _, Album _, Track foreignTrack) = CreateDomainArtist();
        AudioMetadataDto dto = _audioMetadataDtoFixture.Create();

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, foreignTrack, []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    /// <summary>
    /// Creates a domain artist that owns a single album, which in turn owns a single track.
    /// </summary>
    /// <returns>The created domain artist, together with its album and track.</returns>
    private (Artist artist, Album album, Track track) CreateDomainArtist()
    {
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity albumEntity = _albumEntityFixture.Create(
            id: albumId,
            libraryId: libraryId,
            tracks: [_trackEntityFixture.Create(albumId: albumId, libraryId: libraryId)]);
        ArtistEntity artistEntity = _artistEntityFixture.Create(libraryId: libraryId, albums: [albumEntity]);
        Artist artist = artistEntity.ToDomainEntity().Value;
        Album album = artist.Albums.First();
        return (artist, album, album.Tracks.First());
    }
}
