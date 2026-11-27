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
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;

/// <summary>
/// Contains unit tests for the <see cref="DeleteArtistCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteArtistCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<DeleteArtistCommand> _mockValidator;
    private readonly DeleteArtistCommandHandler _sut;
    private readonly Guid _userId;
    private readonly DeleteArtistCommandFixture _deleteArtistCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteArtistCommandHandlerTests"/> class.
    /// </summary>
    public DeleteArtistCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<DeleteArtistCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the deletion, the artist and the
        // library exist, marking the artist as deleted succeeds, and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<DeleteArtistCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(_artistEntityFixture.Create(includeAlbums: false, includeContributors: false)));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockArtistRepository.SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new DeleteArtistCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenArtistExists_ShouldSoftDeleteArtistAndReturnDeleted()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(libraryId, Arg.Any<bool>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: libraryId)));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        // The artist is only marked as deleted, so its metadata, artwork and paths stay available for a later restore.
        await _mockArtistRepository.Received(1).SoftDeleteByIdAsync(artistId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldLoadTheArtistAndTheLibraryWithoutNavigationPropertiesOrTracking()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(libraryId, Arg.Any<bool>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: libraryId)));

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
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteArtistCommand>())
            .Returns([DomainErrors.Music.ArtistIdCannotBeEmpty]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid artistId = Guid.Parse(command.ArtistId!);
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
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
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
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSoftDeleteFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(libraryId, Arg.Any<bool>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: libraryId)));
        _mockArtistRepository.SoftDeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).SoftDeleteByIdAsync(artistId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        DeleteArtistCommand command = _deleteArtistCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockLibraryRepository.GetByIdAsync(libraryId, Arg.Any<bool>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: libraryId)));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).SoftDeleteByIdAsync(artistId, Arg.Any<CancellationToken>());
    }
}
