#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackDeliveryHandleService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryHandleServiceTests
{
    private readonly PlaybackDeliveryHandleService _sut;
    private readonly PlaybackDeliveryHandleDtoFixture _playbackDeliveryHandleDtoFixture = new();
    private readonly NormalizedTrackRequestDtoFixture _normalizedTrackRequestDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackDeliveryHandleServiceTests"/> class.
    /// </summary>
    public PlaybackDeliveryHandleServiceTests()
    {
        _sut = new PlaybackDeliveryHandleService();
    }

    [Fact]
    public void IssueDeliveryId_WhenCalled_ShouldReturnA32CharacterLowercaseHexadecimalHandle()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();

        // Act
        string handle = _sut.IssueDeliveryId(delivery);

        // Assert
        Assert.Equal(32, handle.Length);
        Assert.Matches("^[0-9a-f]{32}$", handle);
        Assert.True(PlaybackDeliveryId.IsValid(handle));
    }

    [Fact]
    public void IssueDeliveryId_WhenDeliveriesDiffer_ShouldReturnDistinctHandles()
    {
        // Arrange
        PlaybackDeliveryHandleDto firstDelivery = CreateBaseDelivery();
        PlaybackDeliveryHandleDto secondDelivery = CreateBaseDelivery();

        // Act
        string firstHandle = _sut.IssueDeliveryId(firstDelivery);
        string secondHandle = _sut.IssueDeliveryId(secondDelivery);

        // Assert
        Assert.NotEqual(firstHandle, secondHandle);
    }

    [Fact]
    public void IssueDeliveryId_WhenTheSameDeliveryIsIssuedAgain_ShouldReturnTheSameHandle()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();

        // Act
        string firstHandle = _sut.IssueDeliveryId(delivery);
        string secondHandle = _sut.IssueDeliveryId(delivery);

        // Assert
        Assert.Equal(firstHandle, secondHandle);
    }

    [Fact]
    public void IssueDeliveryId_WhenAnEqualDeliveryIsIssuedAgainAtThePerUserLimit_ShouldReuseTheExistingHandle()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();
        string handle = _sut.IssueDeliveryId(delivery);
        // Fill the rest of the per user bound with distinct deliveries, so the next issue would evict this one if it did not reuse it.
        for (int index = 0; index < 63; index++)
            _sut.IssueDeliveryId(CreateDeliveryFor(delivery.Request.UserId));
        PlaybackDeliveryHandleDto equalDelivery = delivery with { Request = delivery.Request with { AllowedRoots = [.. delivery.Request.AllowedRoots] } };

        // Act
        string reusedHandle = _sut.IssueDeliveryId(equalDelivery);

        // Assert
        Assert.Equal(handle, reusedHandle);
        Result<PlaybackDeliveryHandleDto> result = _sut.GetDelivery(handle, delivery.Request.UserId, delivery.Request.TrackId);
        Assert.False(result.IsFailure);
        Assert.Same(delivery, result.Value);
    }

    public static TheoryData<DeliveryVariation> SignatureVariations => new()
    {
        DeliveryVariation.UserId,
        DeliveryVariation.TrackId,
        DeliveryVariation.LibraryId,
        DeliveryVariation.AlbumId,
        DeliveryVariation.ArtistId,
        DeliveryVariation.Mode,
        DeliveryVariation.Shape,
        DeliveryVariation.SourceContainer,
        DeliveryVariation.SourceContainerNull,
        DeliveryVariation.Container,
        DeliveryVariation.Codec,
        DeliveryVariation.SegmentDurationSeconds,
        DeliveryVariation.DurationInSeconds,
        DeliveryVariation.Bitrate,
        DeliveryVariation.BitrateNull,
        DeliveryVariation.SampleRate,
        DeliveryVariation.Channels,
        DeliveryVariation.BitDepth,
        DeliveryVariation.BitDepthNull,
        DeliveryVariation.Quality,
        DeliveryVariation.SourcePath,
        DeliveryVariation.SourceLength,
        DeliveryVariation.SourceLastModifiedUtc,
        DeliveryVariation.AllowedRoots
    };

    [Theory]
    [MemberData(nameof(SignatureVariations))]
    public void IssueDeliveryId_WhenOneSignatureInputChanges_ShouldReturnADifferentHandle(DeliveryVariation variation)
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();
        string handle = _sut.IssueDeliveryId(delivery);
        PlaybackDeliveryHandleDto variedDelivery = Vary(delivery, variation);

        // Act
        string variedHandle = _sut.IssueDeliveryId(variedDelivery);

        // Assert
        Assert.NotEqual(handle, variedHandle);
    }

    [Theory]
    [InlineData(null)] // an absent handle cannot address a delivery
    [InlineData("")] // an empty handle cannot address a delivery
    [InlineData("   ")] // a whitespace handle cannot address a delivery
    [InlineData("0123456789abcdef0123456789abcdef")] // a well formed handle that was never issued
    public void GetDelivery_WhenTheHandleIsMissingOrUnknown_ShouldReturnPlaybackDeliveryNotFoundError(string? deliveryId)
    {
        // Act
        Result<PlaybackDeliveryHandleDto> result = _sut.GetDelivery(deliveryId, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.PlaybackDeliveryNotFound, result.FirstError);
    }

    [Fact]
    public void GetDelivery_WhenTheHandleWasIssuedToAnotherUser_ShouldReturnPlaybackDeliveryNotFoundError()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();
        string handle = _sut.IssueDeliveryId(delivery);

        // Act
        Result<PlaybackDeliveryHandleDto> result = _sut.GetDelivery(handle, Guid.NewGuid(), delivery.Request.TrackId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.PlaybackDeliveryNotFound, result.FirstError);
    }

    [Fact]
    public void GetDelivery_WhenTheHandleDescribesAnotherTrack_ShouldReturnPlaybackDeliveryNotFoundError()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();
        string handle = _sut.IssueDeliveryId(delivery);

        // Act
        Result<PlaybackDeliveryHandleDto> result = _sut.GetDelivery(handle, delivery.Request.UserId, Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.PlaybackDeliveryNotFound, result.FirstError);
    }

    [Fact]
    public void GetDelivery_WhenTheHandleWasIssuedToTheUserForTheTrack_ShouldReturnTheIssuedDelivery()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();
        string handle = _sut.IssueDeliveryId(delivery);

        // Act
        Result<PlaybackDeliveryHandleDto> result = _sut.GetDelivery(handle, delivery.Request.UserId, delivery.Request.TrackId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(delivery, result.Value);
        Assert.Equal(delivery.Request.TrackId, result.Value.Request.TrackId);
        Assert.Equal(delivery.Request.UserId, result.Value.Request.UserId);
        Assert.Equal(delivery.Shape, result.Value.Shape);
        Assert.Equal(delivery.LibraryId, result.Value.LibraryId);
        Assert.Equal(delivery.AlbumId, result.Value.AlbumId);
        Assert.Equal(delivery.ArtistId, result.Value.ArtistId);
    }

    [Fact]
    public void IssueDeliveryId_WhenAUserExceedsThePerUserLimit_ShouldEvictOnlyTheOldestHandleOfThatUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        PlaybackDeliveryHandleDto otherUserDelivery = CreateDeliveryFor(otherUserId);
        string otherUserHandle = _sut.IssueDeliveryId(otherUserDelivery);
        List<(string Handle, Guid TrackId)> handles = [];
        for (int index = 0; index < 64; index++)
        {
            PlaybackDeliveryHandleDto delivery = CreateDeliveryFor(userId);
            handles.Add((_sut.IssueDeliveryId(delivery), delivery.Request.TrackId));
        }

        // Act
        PlaybackDeliveryHandleDto newestDelivery = CreateDeliveryFor(userId);
        string newestHandle = _sut.IssueDeliveryId(newestDelivery);

        // Assert
        Assert.True(_sut.GetDelivery(handles[0].Handle, userId, handles[0].TrackId).IsFailure);
        for (int index = 1; index < handles.Count; index++)
            Assert.True(_sut.GetDelivery(handles[index].Handle, userId, handles[index].TrackId).IsSuccess);
        Assert.True(_sut.GetDelivery(newestHandle, userId, newestDelivery.Request.TrackId).IsSuccess);
        Assert.True(_sut.GetDelivery(otherUserHandle, otherUserId, otherUserDelivery.Request.TrackId).IsSuccess);
    }

    [Fact]
    public void IssueDeliveryId_WhenTheGlobalLimitIsExceeded_ShouldEvictOnlyTheOldestHandle()
    {
        // Arrange
        PlaybackDeliveryHandleService sut = new();
        Guid[] users = [.. Enumerable.Range(0, 400).Select(_ => Guid.NewGuid())];
        string? oldestHandle = null;
        Guid oldestUserId = Guid.Empty;
        Guid oldestTrackId = Guid.Empty;
        string? secondOldestHandle = null;
        Guid secondOldestUserId = Guid.Empty;
        Guid secondOldestTrackId = Guid.Empty;
        string? newestHandle = null;
        Guid newestUserId = Guid.Empty;
        Guid newestTrackId = Guid.Empty;
        for (int index = 0; index < 20_000; index++)
        {
            // The per user bound is 64, so each user is given exactly 64 deliveries and never triggers a per user eviction.
            Guid userId = users[index / 64];
            PlaybackDeliveryHandleDto delivery = CreateDeliveryFor(userId);
            string handle = sut.IssueDeliveryId(delivery);
            if (index == 0)
            {
                oldestHandle = handle;
                oldestUserId = userId;
                oldestTrackId = delivery.Request.TrackId;
            }
            else if (index == 1)
            {
                secondOldestHandle = handle;
                secondOldestUserId = userId;
                secondOldestTrackId = delivery.Request.TrackId;
            }
            if (index == 19_999)
            {
                newestHandle = handle;
                newestUserId = userId;
                newestTrackId = delivery.Request.TrackId;
            }
        }
        PlaybackDeliveryHandleDto extraDelivery = CreateDeliveryFor(Guid.NewGuid());

        // Act
        string extraHandle = sut.IssueDeliveryId(extraDelivery);

        // Assert
        Assert.True(sut.GetDelivery(oldestHandle, oldestUserId, oldestTrackId).IsFailure);
        Assert.True(sut.GetDelivery(secondOldestHandle, secondOldestUserId, secondOldestTrackId).IsSuccess);
        Assert.True(sut.GetDelivery(newestHandle, newestUserId, newestTrackId).IsSuccess);
        Assert.True(sut.GetDelivery(extraHandle, extraDelivery.Request.UserId, extraDelivery.Request.TrackId).IsSuccess);
    }

    [Fact]
    public async Task IssueDeliveryId_WhenTheSameDeliveryIsIssuedConcurrently_ShouldReturnOneIdenticalHandle()
    {
        // Arrange
        PlaybackDeliveryHandleDto delivery = CreateBaseDelivery();

        // Act
        string[] handles = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => _sut.IssueDeliveryId(delivery))));

        // Assert
        Assert.Single(handles.Distinct());
        Assert.True(_sut.GetDelivery(handles[0], delivery.Request.UserId, delivery.Request.TrackId).IsSuccess);
    }

    [Fact]
    public async Task IssueDeliveryId_WhenDifferentDeliveriesAreIssuedConcurrently_ShouldMakeEveryHandleResolvable()
    {
        // Arrange
        PlaybackDeliveryHandleDto[] deliveries = [.. Enumerable.Range(0, 32).Select(_ => CreateDeliveryFor(Guid.NewGuid()))];

        // Act
        string[] handles = await Task.WhenAll(deliveries.Select(delivery => Task.Run(() => _sut.IssueDeliveryId(delivery))));

        // Assert
        Assert.Equal(deliveries.Length, handles.Distinct().Count());
        for (int index = 0; index < deliveries.Length; index++)
            Assert.True(_sut.GetDelivery(handles[index], deliveries[index].Request.UserId, deliveries[index].Request.TrackId).IsSuccess);
    }

    /// <summary>
    /// The single input of the delivery signature that a variation changes.
    /// </summary>
    public enum DeliveryVariation
    {
        UserId,
        TrackId,
        LibraryId,
        AlbumId,
        ArtistId,
        Mode,
        Shape,
        SourceContainer,
        SourceContainerNull,
        Container,
        Codec,
        SegmentDurationSeconds,
        DurationInSeconds,
        Bitrate,
        BitrateNull,
        SampleRate,
        Channels,
        BitDepth,
        BitDepthNull,
        Quality,
        SourcePath,
        SourceLength,
        SourceLastModifiedUtc,
        AllowedRoots
    }

    /// <summary>
    /// Creates a delivery with every signature input explicitly pinned.
    /// </summary>
    /// <returns>The created delivery.</returns>
    private PlaybackDeliveryHandleDto CreateBaseDelivery()
    {
        return _playbackDeliveryHandleDtoFixture.Create(
            request: CreateRequest(userId: Guid.NewGuid(), trackId: Guid.NewGuid()),
            shape: PlaybackDeliveryShape.TimeAddressedSegments,
            libraryId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            albumId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            artistId: Guid.Parse("33333333-3333-3333-3333-333333333333"));
    }

    /// <summary>
    /// Creates a delivery for a user whose track is unique to the delivery.
    /// </summary>
    /// <param name="userId">The Id of the user the delivery is issued to.</param>
    /// <returns>The created delivery.</returns>
    private PlaybackDeliveryHandleDto CreateDeliveryFor(Guid userId)
    {
        return _playbackDeliveryHandleDtoFixture.Create(
            request: CreateRequest(userId: userId, trackId: Guid.NewGuid()),
            shape: PlaybackDeliveryShape.TimeAddressedSegments,
            libraryId: Guid.NewGuid(),
            albumId: Guid.NewGuid(),
            artistId: Guid.NewGuid());
    }

    /// <summary>
    /// Changes exactly one input of the delivery signature.
    /// </summary>
    /// <param name="delivery">The delivery to vary.</param>
    /// <param name="variation">The input to change.</param>
    /// <returns>The varied delivery.</returns>
    private static PlaybackDeliveryHandleDto Vary(PlaybackDeliveryHandleDto delivery, DeliveryVariation variation)
    {
        return variation switch
        {
            DeliveryVariation.UserId => delivery with { Request = delivery.Request with { UserId = Guid.NewGuid() } },
            DeliveryVariation.TrackId => delivery with { Request = delivery.Request with { TrackId = Guid.NewGuid() } },
            DeliveryVariation.LibraryId => delivery with { LibraryId = Guid.NewGuid() },
            DeliveryVariation.AlbumId => delivery with { AlbumId = Guid.NewGuid() },
            DeliveryVariation.ArtistId => delivery with { ArtistId = Guid.NewGuid() },
            DeliveryVariation.Mode => delivery with { Request = delivery.Request with { Mode = PlaybackDeliveryMode.Remux } },
            DeliveryVariation.Shape => delivery with { Shape = PlaybackDeliveryShape.ByteRangeSeekable },
            DeliveryVariation.SourceContainer => delivery with { Request = delivery.Request with { SourceContainer = "WAV" } },
            DeliveryVariation.SourceContainerNull => delivery with { Request = delivery.Request with { SourceContainer = null } },
            DeliveryVariation.Container => delivery with { Request = delivery.Request with { Container = "AAC" } },
            DeliveryVariation.Codec => delivery with { Request = delivery.Request with { Codec = "AAC" } },
            DeliveryVariation.SegmentDurationSeconds => delivery with { Request = delivery.Request with { SegmentDurationSeconds = 10 } },
            DeliveryVariation.DurationInSeconds => delivery with { Request = delivery.Request with { DurationInSeconds = 300 } },
            DeliveryVariation.Bitrate => delivery with { Request = delivery.Request with { Bitrate = 320 } },
            DeliveryVariation.BitrateNull => delivery with { Request = delivery.Request with { Bitrate = null } },
            DeliveryVariation.SampleRate => delivery with { Request = delivery.Request with { SampleRate = 96000 } },
            DeliveryVariation.Channels => delivery with { Request = delivery.Request with { Channels = 6 } },
            DeliveryVariation.BitDepth => delivery with { Request = delivery.Request with { BitDepth = 16 } },
            DeliveryVariation.BitDepthNull => delivery with { Request = delivery.Request with { BitDepth = null } },
            DeliveryVariation.Quality => delivery with { Request = delivery.Request with { Quality = PlaybackStreamingQuality.Low } },
            DeliveryVariation.SourcePath => delivery with { Request = delivery.Request with { SourcePath = "/music/incoming/other.flac" } },
            DeliveryVariation.SourceLength => delivery with { Request = delivery.Request with { SourceLength = 999 } },
            DeliveryVariation.SourceLastModifiedUtc => delivery with { Request = delivery.Request with { SourceLastModifiedUtc = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc) } },
            DeliveryVariation.AllowedRoots => delivery with { Request = delivery.Request with { AllowedRoots = [.. delivery.Request.AllowedRoots.Take(1), "/music/other"] } },
            _ => throw new ArgumentOutOfRangeException(nameof(variation), variation, "Unsupported delivery variation.")
        };
    }

    /// <summary>
    /// Creates a normalization request with every signature input explicitly pinned.
    /// </summary>
    /// <param name="userId">The Id of the user the delivery is issued to.</param>
    /// <param name="trackId">The Id of the track the delivery describes.</param>
    /// <returns>The created request.</returns>
    private NormalizedTrackRequestDto CreateRequest(Guid userId, Guid trackId)
    {
        return _normalizedTrackRequestDtoFixture.Create(
            trackId: trackId,
            userId: userId,
            sourcePath: "/music/library/track.flac",
            allowedRoots: ["/music/library", "/music/incoming"],
            sourceContainer: "FLAC",
            mode: PlaybackDeliveryMode.Transcode,
            container: "MP3",
            codec: "MP3",
            segmentDurationSeconds: 6,
            durationInSeconds: 245,
            bitrate: 900,
            sampleRate: 48000,
            channels: 2,
            bitDepth: 24,
            quality: PlaybackStreamingQuality.High,
            sourceLength: 123456,
            sourceLastModifiedUtc: new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
    }
}
