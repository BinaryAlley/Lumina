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
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;

/// <summary>
/// Contains unit tests for the <see cref="DeleteAlbumCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteAlbumCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<DeleteAlbumCommand> _mockValidator;
    private readonly DeleteAlbumCommandHandler _sut;
    private readonly Guid _userId;
    private readonly DeleteAlbumCommandFixture _deleteAlbumCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAlbumCommandHandlerTests"/> class.
    /// </summary>
    public DeleteAlbumCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<DeleteAlbumCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the deletion, the artist, the library
        // and the album exist, marking the album as deleted succeeds, and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<DeleteAlbumCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(_artistEntityFixture.Create(includeAlbums: false, includeContributors: false)));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockAlbumRepository.SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new DeleteAlbumCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumExists_ShouldSoftDeleteAlbumAndReturnDeleted()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId, albumId, Guid.NewGuid());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        // The album is only marked as deleted, so its metadata, artwork and track paths stay available for a later restore.
        await _mockAlbumRepository.Received(1).SoftDeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldLoadTheArtistAggregateAndTheLibraryWithoutTracking()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId, albumId, Guid.NewGuid());

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // The album is a child of the artist aggregate, so it is loaded with the aggregate and not with a separate album read.
        await _mockArtistRepository.Received(1).GetByIdAsync(
            artistId,
            shouldIncludeNavigationProperties: true,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryRepository.Received(1).GetByIdAsync(
            libraryId,
            shouldIncludeNavigationProperties: true,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteAlbumCommand>())
            .Returns([DomainErrors.Music.AlbumIdCannotBeEmpty]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
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
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (_, Guid artistId, _) = GetCommandIds(command);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: Guid.NewGuid(), includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
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
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, _) = GetCommandIds(command);
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
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, _) = GetCommandIds(command);
        StubArtist(libraryId, artistId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, _) = GetCommandIds(command);
        StubArtist(libraryId, artistId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumIsTheOnlyAlbumOfTheArtist_ShouldReturnArtistMustHaveAtLeastOneAlbumWithoutDeleting()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId, albumId);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // The domain rejects the last album before the write is reached.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLastAlbumCheckRacesWithAnotherDelete_ShouldReturnTheStorageFallbackError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId, albumId, Guid.NewGuid());
        // The aggregate still sees a sibling album, but the storage write finds that the sibling was deleted by a concurrent request.
        _mockAlbumRepository.SoftDeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Music.ArtistMustHaveAtLeastOneAlbum);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
        await _mockAlbumRepository.Received(1).SoftDeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, _) = GetCommandIds(command);
        // The artist owns an album, but not the requested one, so the aggregate graph does not contain it.
        StubArtistAndLibrary(libraryId, artistId, Guid.NewGuid());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, _) = GetCommandIds(command);
        // The route artist owns a different album, and the requested album belongs to another artist, so the aggregate graph does not contain it.
        StubArtistAndLibrary(libraryId, artistId, Guid.NewGuid());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSoftDeleteFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId, albumId, Guid.NewGuid());
        _mockAlbumRepository.SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.Received(1).SoftDeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        DeleteAlbumCommand command = _deleteAlbumCommandFixture.Create();
        (Guid libraryId, Guid artistId, Guid albumId) = GetCommandIds(command);
        StubArtistAndLibrary(libraryId, artistId, albumId, Guid.NewGuid());
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.Received(1).SoftDeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Extracts the parsed identifiers carried by <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command whose route identifiers are extracted.</param>
    /// <returns>The parsed library, artist and album identifiers.</returns>
    private static (Guid libraryId, Guid artistId, Guid albumId) GetCommandIds(DeleteAlbumCommand command)
    {
        return (
            Guid.Parse(command.LibraryId!),
            Guid.Parse(command.ArtistId!),
            Guid.Parse(command.AlbumId!));
    }

    /// <summary>
    /// Stubs the artist repository so that the handler finds an artist that belongs to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The Id of the artist.</param>
    /// <param name="ownedAlbumIds">The Ids of the albums the artist owns.</param>
    private ArtistEntity StubArtist(Guid libraryId, Guid artistId, params Guid[] ownedAlbumIds)
    {
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Albums = [.. ownedAlbumIds.Select(albumId => _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false))];
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        return artist;
    }

    /// <summary>
    /// Stubs the artist and library repositories so that the handler finds the artist, its albums and its library.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The Id of the artist.</param>
    /// <param name="ownedAlbumIds">The Ids of the albums the artist owns.</param>
    private void StubArtistAndLibrary(Guid libraryId, Guid artistId, params Guid[] ownedAlbumIds)
    {
        StubArtist(libraryId, artistId, ownedAlbumIds);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId);
        _mockLibraryRepository.GetByIdAsync(libraryId, Arg.Any<bool>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }
}
