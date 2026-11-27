#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackStreamQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackStreamQueryHandlerTests : IDisposable
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IPathService _mockPathService;
    private readonly IValidator<GetTrackStreamQuery> _mockValidator;
    private readonly GetTrackStreamQueryHandler _sut;
    private readonly Guid _userId;
    private readonly string _contentRoot;
    private readonly GetTrackStreamQueryFixture _getTrackStreamQueryFixture = new();
    private readonly TrackStreamDtoFixture _trackStreamDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackStreamQueryHandlerTests"/> class.
    /// </summary>
    public GetTrackStreamQueryHandlerTests()
    {
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockValidator = Substitute.For<IValidator<GetTrackStreamQuery>>();
        _userId = Guid.NewGuid();
        _contentRoot = Path.Combine(Path.GetTempPath(), "lumina-track-stream-handler-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentRoot);

        // Default stubs: the user is authenticated, the library ownership policy allows access, the query validates and every path is inside its content location.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetTrackStreamQuery>()).Returns([]);
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _sut = new GetTrackStreamQueryHandler(
            _mockUnitOfWork,
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockPathService,
            _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutReadingAnything()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetTrackStreamQuery>()).Returns([Errors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetTrackStreamInfoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedWithoutEvaluatingPolicies()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryOwnershipIsDenied_ShouldReturnNotAuthorizedWithoutLookingUpTheTrack()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetTrackStreamInfoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackLookupFails_ShouldReturnItsError()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        _mockTrackRepository.GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            Arg.Any<CancellationToken>())
            .Returns(Errors.Music.TrackFileNotFound);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackFileNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackIsNotFound_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        _mockTrackRepository.GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackStreamDto?>(null));

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRouteIdentifiersDoNotAddressTheTrack_ShouldLookThemUpExactlyAndReturnTrackNotFound()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        _mockTrackRepository.GetTrackStreamInfoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackStreamDto?>(null));

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        await _mockTrackRepository.Received(1).GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryLookupFails_ShouldReturnItsError()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        TrackStreamDto track = _trackStreamDtoFixture.Create(path: Path.Combine(_contentRoot, "track.flac"), libraryId: Guid.Parse(query.LibraryId!));
        StubTrackLookup(query, track);
        _mockLibraryRepository.GetByIdAsync(track.LibraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryIsNotFound_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        TrackStreamDto track = _trackStreamDtoFixture.Create(path: Path.Combine(_contentRoot, "track.flac"), libraryId: Guid.Parse(query.LibraryId!));
        StubTrackLookup(query, track);
        _mockLibraryRepository.GetByIdAsync(track.LibraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheFileIsOutsideEveryContentLocation_ShouldReturnTrackFileNotFoundWithoutLeakingThePath()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        string trackPath = Path.Combine(_contentRoot, "track.flac");
        await File.WriteAllBytesAsync(trackPath, new byte[64]);
        TrackStreamDto track = _trackStreamDtoFixture.Create(path: trackPath, libraryId: Guid.Parse(query.LibraryId!));
        StubTrackLookup(query, track);
        StubLibrary(track.LibraryId, Path.Combine(_contentRoot, "elsewhere"));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackFileNotFound, result.FirstError);
        Assert.DoesNotContain(trackPath, result.FirstError.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenTheFileDoesNotExist_ShouldReturnTrackFileNotFound()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        string trackPath = Path.Combine(_contentRoot, "missing-track.flac");
        TrackStreamDto track = _trackStreamDtoFixture.Create(path: trackPath, libraryId: Guid.Parse(query.LibraryId!));
        StubTrackLookup(query, track);
        StubLibrary(track.LibraryId, _contentRoot);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackFileNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheFileExists_ShouldReturnItsStreamData()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        string trackPath = Path.Combine(_contentRoot, "track.flac");
        byte[] audioBytes = [1, 2, 3, 4, 5, 6, 7, 8];
        await File.WriteAllBytesAsync(trackPath, audioBytes);
        DateTime expectedLastModifiedUtc = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(trackPath, expectedLastModifiedUtc);
        DateTime resolvedLastModifiedUtc = File.GetLastWriteTimeUtc(trackPath);
        TrackStreamDto track = _trackStreamDtoFixture.Create(path: trackPath, libraryId: Guid.Parse(query.LibraryId!));
        StubTrackLookup(query, track);
        StubLibrary(track.LibraryId, _contentRoot);

        // Act
        Result<TrackStreamDataDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(trackPath, result.Value.Path);
        Assert.Equal(AudioFileFormat.FLAC, result.Value.Format);
        Assert.Equal(audioBytes.Length, result.Value.Length);
        Assert.Equal(resolvedLastModifiedUtc, result.Value.LastModified.UtcDateTime);
        _mockPathService.Received(1).IsPathWithin(trackPath, _contentRoot);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheRouteIdentifiersAndCancellationToken()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        TrackStreamDto track = _trackStreamDtoFixture.Create(path: Path.Combine(_contentRoot, "track.flac"), libraryId: Guid.Parse(query.LibraryId!));
        _mockTrackRepository.GetTrackStreamInfoAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackStreamDto?>(track));
        StubLibrary(track.LibraryId, _contentRoot);

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockTrackRepository.Received(1).GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            Arg.Is(cancellationToken));
        await _mockLibraryRepository.Received(1).GetByIdAsync(track.LibraryId, shouldTrackEntities: false, cancellationToken: cancellationToken);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == Guid.Parse(query.LibraryId!)),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequested_ShouldCancelTheOperation()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockTrackRepository.GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<Result<TrackStreamDto?>>(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.HandleAsync(query, cancellationTokenSource.Token));
        await _mockTrackRepository.Received(1).GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stubs the track projection lookup of the query's route with the provided track.
    /// </summary>
    /// <param name="query">The query whose route identifiers are looked up.</param>
    /// <param name="track">The track projection the lookup returns.</param>
    private void StubTrackLookup(GetTrackStreamQuery query, TrackStreamDto track)
    {
        _mockTrackRepository.GetTrackStreamInfoAsync(
            Guid.Parse(query.LibraryId!),
            Guid.Parse(query.AlbumId!),
            Guid.Parse(query.ArtistId!),
            Guid.Parse(query.TrackId!),
            Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackStreamDto?>(track));
    }

    /// <summary>
    /// Stubs the library lookup with a library whose single content location is the provided path.
    /// </summary>
    /// <param name="libraryId">The Id of the library the lookup returns.</param>
    /// <param name="contentLocation">The content location of the returned library.</param>
    private void StubLibrary(Guid libraryId, string contentLocation)
    {
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, contentLocations: [contentLocation]);
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Releases the temporary directory the file tests use.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_contentRoot))
                Directory.Delete(_contentRoot, recursive: true);
        }
        catch (IOException)
        {
            // A temporary directory that cannot be removed does not fail the test.
        }
    }
}
