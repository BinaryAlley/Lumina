#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;

/// <summary>
/// Contains unit tests for the <see cref="RestoreAlbumCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreAlbumCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<RestoreAlbumCommand> _mockValidator;
    private readonly RestoreAlbumCommandHandler _sut;
    private readonly Guid _userId;
    private readonly RestoreAlbumCommandFixture _restoreAlbumCommandFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreAlbumCommandHandlerTests"/> class.
    /// </summary>
    public RestoreAlbumCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<RestoreAlbumCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the restoration, restoring the deleted
        // album succeeds, and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<RestoreAlbumCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockAlbumRepository.RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new RestoreAlbumCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedAlbumExists_ShouldRestoreAlbumAndReturnUpdated()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);
        AlbumEntity album = MarkAsDeleted(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false));
        _mockAlbumRepository.GetSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        await _mockAlbumRepository.Received(1).RestoreSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<RestoreAlbumCommand>())
            .Returns([DomainErrors.Music.AlbumIdCannotBeEmpty]);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumIdCannotBeEmpty, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResultWithoutRestoring()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        _mockAlbumRepository.GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        _mockAlbumRepository.GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(null));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);
        AlbumEntity album = MarkAsDeleted(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: Guid.NewGuid(), includeTracks: false));
        _mockAlbumRepository.GetSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the album exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid albumId = Guid.Parse(command.AlbumId!);
        AlbumEntity album = MarkAsDeleted(_albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId, includeTracks: false));
        _mockAlbumRepository.GetSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the album exists under another artist.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);
        AlbumEntity album = MarkAsDeleted(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false));
        _mockAlbumRepository.GetSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoreFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);
        AlbumEntity album = MarkAsDeleted(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false));
        _mockAlbumRepository.GetSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockAlbumRepository.RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.Received(1).RestoreSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        RestoreAlbumCommand command = _restoreAlbumCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);
        AlbumEntity album = MarkAsDeleted(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false));
        _mockAlbumRepository.GetSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.Received(1).RestoreSoftDeletedByIdAsync(albumId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Marks <paramref name="album"/> as soft deleted, the way the album repository returns it to the restore handler.
    /// </summary>
    /// <param name="album">The album to mark as soft deleted.</param>
    /// <returns>The same <paramref name="album"/> instance, marked as soft deleted.</returns>
    private static AlbumEntity MarkAsDeleted(AlbumEntity album)
    {
        album.IsDeleted = true;
        album.DeletedOnUtc = DateTime.UtcNow;
        return album;
    }
}
