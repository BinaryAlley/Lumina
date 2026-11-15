#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;

/// <summary>
/// Contains unit tests for the <see cref="RestoreTrackCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreTrackCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<RestoreTrackCommand> _mockValidator;
    private readonly RestoreTrackCommandHandler _sut;
    private readonly Guid _userId;
    private readonly RestoreTrackCommandFixture _restoreTrackCommandFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreTrackCommandHandlerTests"/> class.
    /// </summary>
    public RestoreTrackCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<RestoreTrackCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the restoration, restoring the deleted
        // track succeeds, and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<RestoreTrackCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockTrackRepository.RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new RestoreTrackCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedTrackExists_ShouldRestoreTrackAndReturnUpdated()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        await _mockTrackRepository.Received(1).RestoreSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<RestoreTrackCommand>())
            .Returns([DomainErrors.Music.TrackIdCannotBeEmpty]);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackIdCannotBeEmpty, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackRepositoryFails_ShouldReturnFailureResultWithoutRestoring()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        _mockTrackRepository.GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedTrackDoesNotExist_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        _mockTrackRepository.GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<TrackEntity?>(null));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackBelongsToAnotherLibrary_ShouldReturnTrackNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (_, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: Guid.NewGuid(), includeMetadata: false));
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the track exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackBelongsToAnotherAlbum_ShouldReturnTrackNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, _, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: Guid.NewGuid(), libraryId: libraryId, includeMetadata: false));
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the track exists under another album.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResultWithoutRestoring()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, _, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, _, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(null));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // The deleted track is only restorable while its album is visible, so a missing album is reported as a missing track.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, _, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId, includeTracks: false);
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoreFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.Received(1).RestoreSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        RestoreTrackCommand command = _restoreTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        TrackEntity track = MarkAsDeleted(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false));
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        _mockTrackRepository.GetSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.Received(1).RestoreSoftDeletedByIdAsync(trackId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Extracts the parsed identifiers carried by <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command whose route identifiers are extracted.</param>
    /// <returns>The parsed library, artist, album and track identifiers.</returns>
    private static (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) GetCommandIds(RestoreTrackCommand command)
    {
        return (
            Guid.Parse(command.LibraryId!),
            Guid.Parse(command.ArtistId!),
            Guid.Parse(command.AlbumId!),
            Guid.Parse(command.TrackId!));
    }

    /// <summary>
    /// Marks <paramref name="track"/> as soft deleted, the way the track repository returns it to the restore handler.
    /// </summary>
    /// <param name="track">The track to mark as soft deleted.</param>
    /// <returns>The same <paramref name="track"/> instance, marked as soft deleted.</returns>
    private static TrackEntity MarkAsDeleted(TrackEntity track)
    {
        track.IsDeleted = true;
        track.DeletedOnUtc = DateTime.UtcNow;
        return track;
    }
}
