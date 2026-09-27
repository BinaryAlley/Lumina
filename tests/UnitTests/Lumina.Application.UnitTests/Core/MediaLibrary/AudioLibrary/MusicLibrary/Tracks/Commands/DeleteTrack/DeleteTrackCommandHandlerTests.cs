#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Contains unit tests for the <see cref="DeleteTrackCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteTrackCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
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
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<DeleteTrackCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the deletion, the artist update succeeds and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<DeleteTrackCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new DeleteTrackCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenTrackExists_ShouldRemoveTrackFromAlbumAndReturnDeleted()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        StubArtistAndLibrary(artist, libraryId);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockArtistRepository.Received(1).UpdateAsync(
            Arg.Is<ArtistEntity>(updatedArtist =>
                updatedArtist.Id == artistId &&
                updatedArtist.Albums.Count == 1 &&
                updatedArtist.Albums[0].Id == albumId &&
                updatedArtist.Albums[0].Tracks.Count == 0),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldLoadTheArtistWithNavigationPropertiesAndTheLibraryWithoutTracking()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        StubArtistAndLibrary(artist, libraryId);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _mockArtistRepository.Received(1).GetByIdAsync(
            artistId,
            shouldIncludeNavigationProperties: true,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryRepository.Received(1).GetByIdAsync(
            libraryId,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId),
            Arg.Any<CancellationToken>());
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
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
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
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetArtistFails_ShouldReturnError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (_, Guid artistId, _, _) = GetCommandIds(command);
        Error error = Error.Failure(description: "Failed to get artist");
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (_, Guid artistId, _, _) = GetCommandIds(command);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundErrorWithoutEvaluatingThePolicy()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(Guid.NewGuid(), artistId, albumId, trackId);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the artist exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExistInArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, _, Guid trackId) = GetCommandIds(command);
        Guid otherAlbumId = Guid.NewGuid();
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, otherAlbumId, trackId);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackDoesNotExistInAlbum_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, _) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, Guid.NewGuid());
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetLibraryFails_ShouldReturnError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        Error error = Error.Failure(description: "Failed to get library");
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDomainConversionFails_ShouldReturnError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        // An empty name is rejected by the artist aggregate invariant, so the repository entity fails to convert to a domain entity.
        AlbumEntity album = CreateAlbumWithTrack(libraryId, artistId, albumId, trackId);
        ArtistEntity artist = _artistEntityFixture.Create(
            id: artistId,
            libraryId: libraryId,
            name: string.Empty,
            albums: [album],
            includeContributors: false,
            includeMetadata: false);
        StubArtistAndLibrary(artist, libraryId);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNameCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateArtistFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        StubArtistAndLibrary(artist, libraryId);
        Error error = Error.Failure(description: "Failed to update artist");
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = GetCommandIds(command);
        ArtistEntity artist = CreateArtistWithAlbumAndTrack(libraryId, artistId, albumId, trackId);
        StubArtistAndLibrary(artist, libraryId);
        Error error = Error.Failure(description: "Failed to save changes");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockArtistRepository.Received(1).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
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
    /// Creates an artist that contains a single album, which in turn contains a single track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The Id of the artist.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The created artist entity, with its album and track graph.</returns>
    private ArtistEntity CreateArtistWithAlbumAndTrack(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        AlbumEntity album = CreateAlbumWithTrack(libraryId, artistId, albumId, trackId);
        return _artistEntityFixture.Create(
            id: artistId,
            libraryId: libraryId,
            albums: [album],
            includeContributors: false,
            includeMetadata: false);
    }

    /// <summary>
    /// Creates an album that contains a single track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The created album entity, with its track.</returns>
    private AlbumEntity CreateAlbumWithTrack(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId,
            albumId: albumId,
            libraryId: libraryId,
            includeMetadata: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
        return _albumEntityFixture.Create(
            id: albumId,
            artistId: artistId,
            libraryId: libraryId,
            tracks: [track],
            includeMetadata: false,
            includeBarcode: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false);
    }

    /// <summary>
    /// Stubs the artist and library repositories so that the handler finds the artist and the library.
    /// </summary>
    /// <param name="artist">The artist returned by the artist repository.</param>
    /// <param name="libraryId">The Id of the media library returned by the library repository.</param>
    private void StubArtistAndLibrary(ArtistEntity artist, Guid libraryId)
    {
        _mockArtistRepository.GetByIdAsync(artist.Id, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId);
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }
}
