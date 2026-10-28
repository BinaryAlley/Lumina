#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackManifestQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestQueryHandlerTests : IDisposable
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IPlaybackFormatService _mockPlaybackFormatService;
    private readonly IPlaybackDeliveryHandleService _mockPlaybackDeliveryHandleService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IPathService _mockPathService;
    private readonly IValidator<GetPlaybackManifestQuery> _mockValidator;
    private readonly GetPlaybackManifestQueryHandler _sut;
    private readonly Guid _userId;
    private readonly string _issuedDeliveryId;
    private readonly string _sandboxRoot;
    private readonly string _libraryRoot;
    private readonly string _sourcePath;
    private PlaybackDeliveryHandleDto? _issuedDelivery;
    private readonly GetPlaybackManifestQueryFixture _getPlaybackManifestQueryFixture = new();
    private readonly TrackPlaybackManifestDtoFixture _trackPlaybackManifestDtoFixture = new();
    private readonly PlaybackDeliveryDecisionDtoFixture _playbackDeliveryDecisionDtoFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestQueryHandlerTests"/> class.
    /// </summary>
    public GetPlaybackManifestQueryHandlerTests()
    {
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockPlaybackFormatService = Substitute.For<IPlaybackFormatService>();
        _mockPlaybackDeliveryHandleService = Substitute.For<IPlaybackDeliveryHandleService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockValidator = Substitute.For<IValidator<GetPlaybackManifestQuery>>();
        _userId = Guid.NewGuid();
        _issuedDeliveryId = Guid.NewGuid().ToString("N");
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "lumina-playback-manifest-tests-" + Guid.NewGuid().ToString("N"));
        _libraryRoot = Path.Combine(_sandboxRoot, "library");
        _sourcePath = Path.Combine(_libraryRoot, "track.flac");
        Directory.CreateDirectory(_libraryRoot);

        // Default stubs: the user is authenticated, the library ownership policy allows access, the query validates and every path is inside its content location.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetPlaybackManifestQuery>()).Returns([]);
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _sut = new GetPlaybackManifestQueryHandler(
            _mockUnitOfWork,
            _mockPlaybackFormatService,
            _mockPlaybackDeliveryHandleService,
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockPathService,
            _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutReadingAnything()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetPlaybackManifestQuery>()).Returns([Errors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetPlaybackManifestInfoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryOwnershipIsDenied_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetPlaybackManifestInfoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackLookupFails_ShouldReturnItsError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        _mockTrackRepository.GetPlaybackManifestInfoAsync(libraryId, albumId, artistId, trackId, Arg.Any<CancellationToken>())
            .Returns(Errors.Music.TrackFileNotFound);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackFileNotFound, result.FirstError);
        _mockPlaybackFormatService.DidNotReceive().DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackIsNotFound_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        _mockTrackRepository.GetPlaybackManifestInfoAsync(libraryId, albumId, artistId, trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackPlaybackManifestDto?>(null));

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        _mockPlaybackFormatService.DidNotReceive().DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheDeliveryDecisionFails_ShouldReturnItsError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(
            path: _sourcePath, container: "AIFF", audioCodec: "PCM");
        StubTrackLookup(query, track);
        _mockPlaybackFormatService.DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>())
            .Returns(Errors.Playback.NoSupportedPlaybackFormat);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.NoSupportedPlaybackFormat, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheStoredAudioIsPlayedDirectly_ShouldReturnTheStoredFieldsWithoutIssuingADelivery()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(
            path: _sourcePath,
            container: "FLAC",
            audioCodec: "FLAC",
            durationInSeconds: 245,
            sampleRate: 48000,
            channels: 2,
            bitDepth: 24,
            replayGainTrackGain: -6.5m,
            replayGainTrackPeak: 0.98m,
            replayGainAlbumGain: -7.1m,
            replayGainAlbumPeak: 0.97m);
        PlaybackDeliveryDecisionDto decision = _playbackDeliveryDecisionDtoFixture.Create(
            mode: PlaybackDeliveryMode.DirectPlay,
            container: "FLAC",
            codec: "FLAC",
            shape: PlaybackDeliveryShape.ByteRangeSeekable,
            segmentDurationSeconds: 0);
        StubTrackLookup(query, track);
        _mockPlaybackFormatService.DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>())
            .Returns(Result.From(decision));

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Guid.Parse(query.TrackId!), result.Value.TrackId);
        Assert.Equal(245, result.Value.DurationInSeconds);
        Assert.Equal("FLAC", result.Value.SourceContainer);
        Assert.Equal("FLAC", result.Value.SourceCodec);
        Assert.Equal(48000, result.Value.SampleRate);
        Assert.Equal(2, result.Value.Channels);
        Assert.Equal(24, result.Value.BitDepth);
        Assert.Equal(-6.5m, result.Value.ReplayGainTrackGain);
        Assert.Equal(0.98m, result.Value.ReplayGainTrackPeak);
        Assert.Equal(-7.1m, result.Value.ReplayGainAlbumGain);
        Assert.Equal(0.97m, result.Value.ReplayGainAlbumPeak);
        Assert.True(result.Value.IsDirectPlay);
        Assert.Null(result.Value.DeliveryContainer);
        Assert.Null(result.Value.DeliveryCodec);
        Assert.Null(result.Value.DeliveryShape);
        Assert.Null(result.Value.SegmentDurationSeconds);
        Assert.True(result.Value.IsSeekable);
        Assert.Equal(query.StreamPath, result.Value.StreamPath);
        Assert.Null(result.Value.DeliveryId);
        Assert.Null(result.Value.SegmentStreamPathTemplate);
        _mockPlaybackDeliveryHandleService.DidNotReceive().IssueDeliveryId(Arg.Any<PlaybackDeliveryHandleDto>());
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserHasNoSettings_ShouldCapTheNormalizedDeliveryAtAutoQuality()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubNormalizedDelivery(query, track, settings: null);
        CaptureIssuedDelivery();

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(_issuedDelivery);
        Assert.Equal(PlaybackStreamingQuality.Auto, _issuedDelivery.Request.Quality);
        await _mockUserSettingsRepository.Received(1).GetByUserIdAsync(_userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserSettingsCannotBeRead_ShouldReturnTheirError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubTrackLookup(query, track);
        StubNormalizedDecision();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Errors.UserSettings.UserSettingsNotFound);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.UserSettingsNotFound, result.FirstError);
        _mockPlaybackDeliveryHandleService.DidNotReceive().IssueDeliveryId(Arg.Any<PlaybackDeliveryHandleDto>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserHasSettings_ShouldCaptureTheirStreamingQualityInTheDelivery()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(userId: _userId, streamingQuality: PlaybackStreamingQuality.High);
        StubNormalizedDelivery(query, track, settings: settings);
        CaptureIssuedDelivery();

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(_issuedDelivery);
        Assert.Equal(PlaybackStreamingQuality.High, _issuedDelivery.Request.Quality);
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryCannotBeRead_ShouldReturnItsError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubTrackLookup(query, track);
        StubNormalizedDecision();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Result.From<UserSettingsEntity?>(null));
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        _mockPlaybackDeliveryHandleService.DidNotReceive().IssueDeliveryId(Arg.Any<PlaybackDeliveryHandleDto>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryIsNotFound_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubTrackLookup(query, track);
        StubNormalizedDecision();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Result.From<UserSettingsEntity?>(null));
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        _mockPlaybackDeliveryHandleService.DidNotReceive().IssueDeliveryId(Arg.Any<PlaybackDeliveryHandleDto>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheSourceFileIsInsideAContentLocation_ShouldCaptureItsIdentityInTheDelivery()
    {
        // Arrange
        byte[] sourceBytes = new byte[1234];
        await File.WriteAllBytesAsync(_sourcePath, sourceBytes);
        DateTime sourceLastModifiedUtc = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(_sourcePath, sourceLastModifiedUtc);
        DateTime expectedLastModifiedUtc = File.GetLastWriteTimeUtc(_sourcePath);
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubNormalizedDelivery(query, track, settings: null);
        CaptureIssuedDelivery();

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(_issuedDelivery);
        Assert.Equal(sourceBytes.Length, _issuedDelivery.Request.SourceLength);
        Assert.Equal(expectedLastModifiedUtc, _issuedDelivery.Request.SourceLastModifiedUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenTheSourceFileIsOutsideEveryContentLocation_ShouldNotCaptureItsIdentity()
    {
        // Arrange
        await File.WriteAllBytesAsync(_sourcePath, new byte[1234]);
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubNormalizedDelivery(query, track, settings: null);
        CaptureIssuedDelivery();
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(_issuedDelivery);
        Assert.Equal(0, _issuedDelivery.Request.SourceLength);
        Assert.Equal(DateTime.MinValue, _issuedDelivery.Request.SourceLastModifiedUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenANormalizedDeliveryIsIssued_ShouldIssueTheExactDeclaredDelivery()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create(capabilities: "mp3/mp3,flac/flac");
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(
            path: _sourcePath,
            container: "FLAC",
            audioCodec: "FLAC",
            durationInSeconds: 245,
            sampleRate: 48000,
            channels: 2,
            bitDepth: 24,
            bitrate: 900);
        PlaybackDeliveryDecisionDto decision = _playbackDeliveryDecisionDtoFixture.Create(
            mode: PlaybackDeliveryMode.Transcode,
            container: "MP3",
            codec: "MP3",
            shape: PlaybackDeliveryShape.TimeAddressedSegments,
            segmentDurationSeconds: 6);
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(userId: _userId, streamingQuality: PlaybackStreamingQuality.High);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, contentLocations: [_libraryRoot]);
        StubTrackLookup(query, track);
        _mockPlaybackFormatService.DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>())
            .Returns(Result.From(decision));
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Result.From<UserSettingsEntity?>(settings));
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        CaptureIssuedDelivery();

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(_issuedDelivery);
        Assert.Equal(trackId, _issuedDelivery.Request.TrackId);
        Assert.Equal(_userId, _issuedDelivery.Request.UserId);
        Assert.Equal(PlaybackDeliveryMode.Transcode, _issuedDelivery.Request.Mode);
        Assert.Equal("MP3", _issuedDelivery.Request.Container);
        Assert.Equal("MP3", _issuedDelivery.Request.Codec);
        Assert.Equal(6, _issuedDelivery.Request.SegmentDurationSeconds);
        Assert.Equal(245, _issuedDelivery.Request.DurationInSeconds);
        Assert.Equal(900, _issuedDelivery.Request.Bitrate);
        Assert.Equal(48000, _issuedDelivery.Request.SampleRate);
        Assert.Equal(2, _issuedDelivery.Request.Channels);
        Assert.Equal(24, _issuedDelivery.Request.BitDepth);
        Assert.Equal(PlaybackStreamingQuality.High, _issuedDelivery.Request.Quality);
        Assert.Equal(_sourcePath, _issuedDelivery.Request.SourcePath);
        Assert.Equal([_libraryRoot], _issuedDelivery.Request.AllowedRoots);
        Assert.Equal(PlaybackDeliveryShape.TimeAddressedSegments, _issuedDelivery.Shape);
        Assert.Equal(libraryId, _issuedDelivery.LibraryId);
        Assert.Equal(albumId, _issuedDelivery.AlbumId);
        Assert.Equal(artistId, _issuedDelivery.ArtistId);
        _mockPlaybackFormatService.Received(1).DecideDelivery(
            Arg.Is<PlaybackSourceFormatDto>(format => format.Container == "FLAC" && format.Codec == "FLAC"),
            Arg.Is<IReadOnlyList<PlaybackCapabilityDto>>(capabilities => capabilities.Count == 2));
    }

    [Fact]
    public async Task HandleAsync_WhenATimeAddressedDeliveryIsIssued_ShouldReturnItsDeliveryShapeAndSeekMap()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        StubNormalizedDelivery(query, track, settings: null);
        CaptureIssuedDelivery();

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.IsDirectPlay);
        Assert.Equal(PlaybackDeliveryShape.TimeAddressedSegments, result.Value.DeliveryShape);
        Assert.Equal(6, result.Value.SegmentDurationSeconds);
        Assert.True(result.Value.IsSeekable);
        Assert.Equal(_issuedDeliveryId, result.Value.DeliveryId);
        Assert.Equal(query.SegmentStreamPathTemplate, result.Value.SegmentStreamPathTemplate);
        Assert.Equal(query.StreamPath, result.Value.StreamPath);
    }

    [Fact]
    public async Task HandleAsync_WhenANonTimeAddressedDeliveryIsIssued_ShouldNotReportASegmentDuration()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        TrackPlaybackManifestDto track = _trackPlaybackManifestDtoFixture.Create(path: _sourcePath);
        PlaybackDeliveryDecisionDto decision = _playbackDeliveryDecisionDtoFixture.Create(
            mode: PlaybackDeliveryMode.Remux,
            container: "WAV",
            codec: "PCM",
            shape: PlaybackDeliveryShape.ByteRangeSeekable,
            segmentDurationSeconds: 0);
        StubTrackLookup(query, track);
        _mockPlaybackFormatService.DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>())
            .Returns(Result.From(decision));
        StubSettings(null);
        StubLibrary(Guid.Parse(query.LibraryId!));
        CaptureIssuedDelivery();

        // Act
        Result<PlaybackManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.IsDirectPlay);
        Assert.Equal(PlaybackDeliveryShape.ByteRangeSeekable, result.Value.DeliveryShape);
        Assert.Null(result.Value.SegmentDurationSeconds);
        Assert.True(result.Value.IsSeekable);
        Assert.Equal(_issuedDeliveryId, result.Value.DeliveryId);
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequestedWhileLookingUpTheTrack_ShouldCancelTheOperation()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockTrackRepository.GetPlaybackManifestInfoAsync(libraryId, albumId, artistId, trackId, cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<Result<TrackPlaybackManifestDto?>>(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.HandleAsync(query, cancellationTokenSource.Token));
        await _mockTrackRepository.Received(1).GetPlaybackManifestInfoAsync(libraryId, albumId, artistId, trackId, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stubs the track projection lookup of the query's route with the provided track.
    /// </summary>
    /// <param name="query">The query whose route identifiers are looked up.</param>
    /// <param name="track">The track projection the lookup returns.</param>
    private void StubTrackLookup(GetPlaybackManifestQuery query, TrackPlaybackManifestDto track)
    {
        _mockTrackRepository.GetPlaybackManifestInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackPlaybackManifestDto?>(track));
    }

    /// <summary>
    /// Stubs a non direct play delivery decision that produces time addressed six second segments.
    /// </summary>
    private void StubNormalizedDecision()
    {
        PlaybackDeliveryDecisionDto decision = _playbackDeliveryDecisionDtoFixture.Create(
            mode: PlaybackDeliveryMode.Transcode,
            container: "MP3",
            codec: "MP3",
            shape: PlaybackDeliveryShape.TimeAddressedSegments,
            segmentDurationSeconds: 6);
        _mockPlaybackFormatService.DecideDelivery(Arg.Any<PlaybackSourceFormatDto>(), Arg.Any<IReadOnlyList<PlaybackCapabilityDto>>())
            .Returns(Result.From(decision));
    }

    /// <summary>
    /// Stubs the user settings lookup with the provided settings, or with no settings when it is <see langword="null"/>.
    /// </summary>
    /// <param name="settings">The settings the lookup returns.</param>
    private void StubSettings(UserSettingsEntity? settings)
    {
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Result.From(settings));
    }

    /// <summary>
    /// Stubs the library lookup with a library that has the test source directory as its content location.
    /// </summary>
    /// <param name="libraryId">The Id of the library the lookup returns.</param>
    private void StubLibrary(Guid libraryId)
    {
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, contentLocations: [_libraryRoot]);
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Stubs the complete non direct play chain of the handler.
    /// </summary>
    /// <param name="query">The query whose route identifiers are looked up.</param>
    /// <param name="track">The track projection the lookup returns.</param>
    /// <param name="settings">The settings the user settings lookup returns.</param>
    private void StubNormalizedDelivery(GetPlaybackManifestQuery query, TrackPlaybackManifestDto track, UserSettingsEntity? settings)
    {
        StubTrackLookup(query, track);
        StubNormalizedDecision();
        StubSettings(settings);
        StubLibrary(Guid.Parse(query.LibraryId!));
    }

    /// <summary>
    /// Configures the delivery handle service so that the delivery it is asked to issue is captured and a fixed handle is returned.
    /// </summary>
    private void CaptureIssuedDelivery()
    {
        _mockPlaybackDeliveryHandleService.IssueDeliveryId(Arg.Do<PlaybackDeliveryHandleDto>(delivery => _issuedDelivery = delivery))
            .Returns(_issuedDeliveryId);
    }

    /// <summary>
    /// Releases the temporary directory the containment tests use.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_sandboxRoot))
                Directory.Delete(_sandboxRoot, recursive: true);
        }
        catch (IOException)
        {
            // A temporary directory that cannot be removed does not fail the test.
        }
    }
}
