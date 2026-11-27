#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;

/// <summary>
/// Contains unit tests for the <see cref="RestoreArtistCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreArtistCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<RestoreArtistCommand> _mockValidator;
    private readonly RestoreArtistCommandHandler _sut;
    private readonly Guid _userId;
    private readonly RestoreArtistCommandFixture _restoreArtistCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreArtistCommandHandlerTests"/> class.
    /// </summary>
    public RestoreArtistCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<RestoreArtistCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the restoration, restoring the deleted
        // artist succeeds, no visible artist took over its name, and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<RestoreArtistCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockArtistRepository.GetByNameAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));
        _mockArtistRepository.RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new RestoreArtistCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedArtistExists_ShouldRestoreArtistAndReturnUpdated()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false));
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        await _mockArtistRepository.Received(1).RestoreSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).GetByNameAsync(libraryId, artist.Name, Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<RestoreArtistCommand>())
            .Returns([DomainErrors.Music.ArtistIdCannotBeEmpty]);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutRestoring()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        _mockArtistRepository.GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        _mockArtistRepository.GetSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: Guid.NewGuid(), includeAlbums: false, includeContributors: false));
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the artist exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetByNameAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false));
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetByNameAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenVisibleArtistTookOverTheName_ShouldReturnArtistAlreadyExistsError()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false));
        ArtistEntity artistWithSameName = _artistEntityFixture.Create(libraryId: libraryId, name: artist.Name, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockArtistRepository.GetByNameAsync(libraryId, artist.Name, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artistWithSameName));

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // The name of a visible artist is unique within a library, so the deleted artist cannot be restored until it is renamed.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistAlreadyExists, result.FirstError);
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetArtistByNameFails_ShouldReturnFailureResultWithoutRestoring()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false));
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockArtistRepository.GetByNameAsync(libraryId, artist.Name, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoreFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false));
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockArtistRepository.RestoreSoftDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).RestoreSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        RestoreArtistCommand command = _restoreArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = MarkAsDeleted(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false));
        _mockArtistRepository.GetSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).RestoreSoftDeletedByIdAsync(artistId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Marks <paramref name="artist"/> as soft deleted, the way the artist repository returns it to the restore handler.
    /// </summary>
    /// <param name="artist">The artist to mark as soft deleted.</param>
    /// <returns>The same <paramref name="artist"/> instance, marked as soft deleted.</returns>
    private static ArtistEntity MarkAsDeleted(ArtistEntity artist)
    {
        artist.IsDeleted = true;
        artist.DeletedOnUtc = DateTime.UtcNow;
        return artist;
    }
}
