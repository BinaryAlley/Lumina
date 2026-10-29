#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackSegmentQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentQueryHandlerTests
{
    private readonly ITrackNormalizationService _mockTrackNormalizationService;
    private readonly IPlaybackDeliveryHandleService _mockPlaybackDeliveryHandleService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IPathService _mockPathService;
    private readonly IValidator<GetTrackSegmentQuery> _mockValidator;
    private readonly GetTrackSegmentQueryHandler _sut;
    private readonly Guid _userId;
    private readonly string _allowedRoot;
    private readonly GetTrackSegmentQueryFixture _getTrackSegmentQueryFixture = new();
    private readonly PlaybackDeliveryHandleDtoFixture _playbackDeliveryHandleDtoFixture = new();
    private readonly NormalizedTrackRequestDtoFixture _normalizedTrackRequestDtoFixture = new();
    private readonly NormalizedTrackDtoFixture _normalizedTrackDtoFixture = new();
    private readonly NormalizedTrackSegmentDtoFixture _normalizedTrackSegmentDtoFixture = new();

    public static TheoryData<string> DeliveryMismatchCases => ["Library", "Album", "Artist"];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentQueryHandlerTests"/> class.
    /// </summary>
    public GetTrackSegmentQueryHandlerTests()
    {
        _mockTrackNormalizationService = Substitute.For<ITrackNormalizationService>();
        _mockPlaybackDeliveryHandleService = Substitute.For<IPlaybackDeliveryHandleService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockValidator = Substitute.For<IValidator<GetTrackSegmentQuery>>();
        _userId = Guid.NewGuid();
        _allowedRoot = Path.Combine(Path.GetTempPath(), "lumina-tests", "music");

        // Default stubs: the user is authenticated, the library ownership policy allows access, the query validates and the source path is inside an allowed root.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetTrackSegmentQuery>()).Returns([]);
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _sut = new GetTrackSegmentQueryHandler(
            _mockTrackNormalizationService,
            _mockPlaybackDeliveryHandleService,
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockPathService,
            _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutResolvingTheDelivery()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetTrackSegmentQuery>()).Returns([Errors.Playback.SegmentIndexMustBeValid]);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.SegmentIndexMustBeValid, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        _mockPlaybackDeliveryHandleService.DidNotReceive().GetDelivery(Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<Guid>());
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        _mockPlaybackDeliveryHandleService.DidNotReceive().GetDelivery(Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryOwnershipIsDenied_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        _mockPlaybackDeliveryHandleService.DidNotReceive().GetDelivery(Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<Guid>());
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheDeliveryCannotBeResolved_ShouldReturnItsErrorWithoutProducingAudio()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        Guid trackId = Guid.Parse(query.TrackId!);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Errors.Playback.PlaybackDeliveryNotFound);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.PlaybackDeliveryNotFound, result.FirstError);
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(DeliveryMismatchCases))]
    public async Task HandleAsync_WhenTheDeliveryDoesNotDescribeTheRoute_ShouldReturnPlaybackDeliveryNotFoundError(string mismatch)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        delivery = mismatch switch
        {
            "Library" => delivery with { LibraryId = Guid.NewGuid() },
            "Album" => delivery with { AlbumId = Guid.NewGuid() },
            _ => delivery with { ArtistId = Guid.NewGuid() }
        };
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.PlaybackDeliveryNotFound, result.FirstError);
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheDeliveryIsADirectPlay_ShouldReturnNormalizationIsNotRequiredForDirectPlayError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query, mode: PlaybackDeliveryMode.DirectPlay);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.NormalizationIsNotRequiredForDirectPlay, result.FirstError);
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    public static TheoryData<PlaybackDeliveryShape, int> NonSegmentDeliveryCases => new()
    {
        { PlaybackDeliveryShape.ByteRangeSeekable, 6 }, // a byte range delivery has no indexable segment
        { PlaybackDeliveryShape.TimeAddressedSegments, 0 } // a time addressed delivery without a positive segment duration has nothing to index
    };

    [Theory]
    [MemberData(nameof(NonSegmentDeliveryCases))]
    public async Task HandleAsync_WhenTheDeliveryIsNotAddressedAsTimeAddressedSegments_ShouldReturnPlaybackDeliveryChangedError(PlaybackDeliveryShape shape, int segmentDurationSeconds)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query, shape: shape, segmentDurationSeconds: segmentDurationSeconds);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.PlaybackDeliveryChanged, result.FirstError);
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheSourcePathIsOutsideEveryAllowedRoot_ShouldReturnTrackFileNotFoundError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackFileNotFound, result.FirstError);
        await _mockTrackNormalizationService.DidNotReceive().GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNormalizationFails_ShouldReturnItsErrorWithoutOpeningASegment()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create();
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockTrackNormalizationService.GetOrCreateNormalizedTrackAsync(delivery.Request, Arg.Any<CancellationToken>())
            .Returns(Errors.Playback.NormalizationFailed);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.NormalizationFailed, result.FirstError);
        _mockTrackNormalizationService.DidNotReceive().OpenNormalizedSegment(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<int>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheNormalizedTrackHasNoRequestedSegment_ShouldReturnNormalizedTrackSegmentNotFoundError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(segmentIndex: "5");
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        NormalizedTrackDto normalizedTrack = _normalizedTrackDtoFixture.Create(
            trackId: trackId,
            segments: [_normalizedTrackSegmentDtoFixture.Create(index: 0)]);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockTrackNormalizationService.GetOrCreateNormalizedTrackAsync(delivery.Request, Arg.Any<CancellationToken>())
            .Returns(Result.From(normalizedTrack));

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.NormalizedTrackSegmentNotFound, result.FirstError);
        _mockTrackNormalizationService.DidNotReceive().OpenNormalizedSegment(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<int>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheSegmentCannotBeOpened_ShouldReturnItsError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(segmentIndex: "0");
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        NormalizedTrackDto normalizedTrack = _normalizedTrackDtoFixture.Create(
            trackId: trackId,
            segments: [_normalizedTrackSegmentDtoFixture.Create(index: 0)]);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockTrackNormalizationService.GetOrCreateNormalizedTrackAsync(delivery.Request, Arg.Any<CancellationToken>())
            .Returns(Result.From(normalizedTrack));
        _mockTrackNormalizationService.OpenNormalizedSegment(normalizedTrack.CacheId, trackId, 0)
            .Returns(Errors.Playback.NormalizedTrackCacheEntryNotFound);

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.NormalizedTrackCacheEntryNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnTheSegmentDataOfTheProducedTrack()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(segmentIndex: "2");
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        NormalizedTrackSegmentDto segment = _normalizedTrackSegmentDtoFixture.Create(index: 2, startSeconds: 12.5m, durationSeconds: 6m, length: 4096);
        NormalizedTrackDto normalizedTrack = _normalizedTrackDtoFixture.Create(
            trackId: trackId,
            container: "MP3",
            segmentDurationSeconds: 6,
            durationInSeconds: 121,
            segments: [_normalizedTrackSegmentDtoFixture.Create(index: 0), _normalizedTrackSegmentDtoFixture.Create(index: 1), segment]);
        MemoryStream stream = new([1, 2, 3, 4]);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockTrackNormalizationService.GetOrCreateNormalizedTrackAsync(delivery.Request, Arg.Any<CancellationToken>())
            .Returns(Result.From(normalizedTrack));
        _mockTrackNormalizationService.OpenNormalizedSegment(normalizedTrack.CacheId, trackId, 2)
            .Returns(Result<Stream>.Success(stream));

        // Act
        Result<NormalizedTrackSegmentDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(stream, result.Value.Content);
        Assert.Equal(segment.Length, result.Value.Length);
        Assert.Equal(segment.StartSeconds, result.Value.StartSeconds);
        Assert.Equal(normalizedTrack.Container, result.Value.Container);
        Assert.Equal(normalizedTrack.Segments.Count, result.Value.SegmentCount);
        Assert.Equal(normalizedTrack.DurationInSeconds, result.Value.DurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldNormalizeTheDeliveryRequestAndOpenTheRequestedSegment()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(segmentIndex: "3");
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        NormalizedTrackSegmentDto segment = _normalizedTrackSegmentDtoFixture.Create(index: 3);
        NormalizedTrackDto normalizedTrack = _normalizedTrackDtoFixture.Create(
            trackId: trackId,
            segmentDurationSeconds: 6,
            segments: [_normalizedTrackSegmentDtoFixture.Create(index: 3)]);
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockTrackNormalizationService.GetOrCreateNormalizedTrackAsync(Arg.Any<NormalizedTrackRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(normalizedTrack));
        _mockTrackNormalizationService.OpenNormalizedSegment(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<int>())
            .Returns(Result<Stream>.Success(new MemoryStream()));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockTrackNormalizationService.Received(1).GetOrCreateNormalizedTrackAsync(
            Arg.Is<NormalizedTrackRequestDto>(request => ReferenceEquals(request, delivery.Request)),
            Arg.Any<CancellationToken>());
        _mockTrackNormalizationService.Received(1).OpenNormalizedSegment(normalizedTrack.CacheId, trackId, segment.Index);
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequestedWhileNormalizing_ShouldCancelTheOperation()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(segmentIndex: "0");
        Guid trackId = Guid.Parse(query.TrackId!);
        PlaybackDeliveryHandleDto delivery = CreateDelivery(query);
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockPlaybackDeliveryHandleService.GetDelivery(query.DeliveryId, _userId, trackId).Returns(Result.From(delivery));
        _mockTrackNormalizationService.GetOrCreateNormalizedTrackAsync(delivery.Request, cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<Result<NormalizedTrackDto>>(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.HandleAsync(query, cancellationTokenSource.Token));
        await _mockTrackNormalizationService.Received(1).GetOrCreateNormalizedTrackAsync(delivery.Request, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Creates a delivery that matches the library, album, artist and track named by the provided query.
    /// </summary>
    /// <param name="query">The query whose route identifiers the delivery must describe.</param>
    /// <param name="mode">The way the audio is delivered.</param>
    /// <param name="shape">The shape the delivered audio is addressed by.</param>
    /// <param name="segmentDurationSeconds">The duration of each segment of the delivered audio, in seconds.</param>
    /// <returns>The created delivery.</returns>
    private PlaybackDeliveryHandleDto CreateDelivery(
        GetTrackSegmentQuery query,
        PlaybackDeliveryMode mode = PlaybackDeliveryMode.Transcode,
        PlaybackDeliveryShape shape = PlaybackDeliveryShape.TimeAddressedSegments,
        int segmentDurationSeconds = 6)
    {
        return _playbackDeliveryHandleDtoFixture.Create(
            request: _normalizedTrackRequestDtoFixture.Create(
                trackId: Guid.Parse(query.TrackId!),
                userId: _userId,
                sourcePath: Path.Combine(_allowedRoot, "track.flac"),
                allowedRoots: [_allowedRoot],
                mode: mode,
                segmentDurationSeconds: segmentDurationSeconds,
                durationInSeconds: 120),
            shape: shape,
            libraryId: Guid.Parse(query.LibraryId!),
            albumId: Guid.Parse(query.AlbumId!),
            artistId: Guid.Parse(query.ArtistId!));
    }
}
