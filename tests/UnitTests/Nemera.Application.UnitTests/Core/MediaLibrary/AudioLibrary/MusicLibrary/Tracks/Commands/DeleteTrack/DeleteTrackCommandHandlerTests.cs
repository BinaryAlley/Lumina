#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Contains unit tests for the <see cref="DeleteTrackCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteTrackCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<DeleteTrackCommand> _mockValidator;
    private readonly DeleteTrackCommandHandler _sut;
    private readonly DeleteTrackCommandFixture _deleteTrackCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTrackCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTrackCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<DeleteTrackCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the deletion, the artist, the library,
        // the album and the track exist, marking the track as deleted succeeds, and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<DeleteTrackCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(_artistEntityFixture.Create(includeAlbums: false, includeContributors: false)));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(_albumEntityFixture.Create(includeTracks: false)));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<TrackEntity?>(_trackEntityFixture.Create(includeMetadata: false)));
        _mockTrackRepository.SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new DeleteTrackCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenTrackExists_ShouldSoftDeleteTrackAndReturnDeleted()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false);
        _mockTrackRepository.GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        // The track is only marked as deleted, so its metadata, artwork and path stay available for a later restore.
        await _mockTrackRepository.Received(1).SoftDeleteByIdAsync(trackId, Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldLoadTheScopingRowsWithoutNavigationPropertiesOrTracking()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false);
        _mockTrackRepository.GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _mockArtistRepository.Received(1).GetByIdAsync(
            artistId,
            shouldIncludeNavigationProperties: false,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryRepository.Received(1).GetByIdAsync(
            libraryId,
            shouldIncludeNavigationProperties: true,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockAlbumRepository.Received(1).GetByIdAsync(
            albumId,
            shouldIncludeNavigationProperties: false,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockTrackRepository.Received(1).GetByIdAsync(
            trackId,
            shouldIncludeNavigationProperties: false,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteTrackCommand>())
            .Returns([DomainErrors.Music.TrackIdCannotBeEmpty]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (_, Guid artistId, _, _) = GetCommandIds(command);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: Guid.NewGuid(), includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the artist exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, _, _) = GetCommandIds(command);
        StubArtist(libraryId, artistId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, _, _) = GetCommandIds(command);
        StubArtist(libraryId, artistId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, _, _) = GetCommandIds(command);
        StubArtist(libraryId, artistId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, _, _) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, _, _) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, _) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId, includeTracks: false);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, _) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackDoesNotExist_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, _) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<TrackEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackBelongsToAnotherAlbum_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: Guid.NewGuid(), libraryId: libraryId, includeMetadata: false);
        _mockTrackRepository.GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSoftDeleteFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false);
        _mockTrackRepository.GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockTrackRepository.SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.Received(1).SoftDeleteByIdAsync(trackId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId);
        StubAlbum(libraryId, artistId, albumId);
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false);
        _mockTrackRepository.GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.Received(1).SoftDeleteByIdAsync(trackId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Extracts the parsed identifiers carried by <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command whose route identifiers are extracted.</param>
    /// <returns>The parsed library, artist, album and track identifiers.</returns>
    private static (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) GetCommandIds(DeleteTrackCommand command)
    {
        return (
            Guid.Parse(command.LibraryId!),
            Guid.Parse(command.ArtistId!),
            Guid.Parse(command.AlbumId!),
            Guid.Parse(command.TrackId!));
    }

    /// <summary>
    /// Stubs the artist repository so that the handler finds an artist that belongs to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The Id of the artist.</param>
    private void StubArtist(Guid libraryId, Guid artistId)
    {
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
    }

    /// <summary>
    /// Stubs the album repository so that the handler finds an album of <paramref name="artistId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    private void StubAlbum(Guid libraryId, Guid artistId, Guid albumId)
    {
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
    }

    /// <summary>
    /// Stubs the artist and library repositories so that the handler finds the artist and its library.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The Id of the artist.</param>
    private void StubArtistAndLibrary(Guid libraryId, Guid artistId)
    {
        StubArtist(libraryId, artistId);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId);
        _mockLibraryRepository.GetByIdAsync(libraryId, Arg.Any<bool>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }
}
