#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateArtistCommand> _mockValidator;
    private readonly IPathService _mockPathService;
    private readonly UpdateArtistCommandHandler _sut;
    private readonly Guid _userId;
    private readonly UpdateArtistCommandFixture _updateArtistCommandFixture = new();
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCommandHandlerTests"/> class.
    /// </summary>
    public UpdateArtistCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<UpdateArtistCommand>>();
        _mockPathService = Substitute.For<IPathService>();
        _userId = Guid.NewGuid();

        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        // Default stub: saving changes succeeds.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        // Default stubs: the validator passes, the current user is authenticated and the library ownership policy allows access.
        _mockValidator.Validate(Arg.Any<UpdateArtistCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        // Default stubs: the library exists and the track paths are inside its content locations.
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);
        // Default stub: every referenced media contributor already exists.
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                IReadOnlyCollection<Guid> contributorIds = callInfo.Arg<IReadOnlyCollection<Guid>>();
                return Result.From<IReadOnlyList<MediaContributorEntity>>([.. contributorIds.Select(contributorId => _mediaContributorEntityFixture.Create(id: contributorId, displayName: contributorId.ToString()))]);
            });
        // Default stub: updating the artist aggregate succeeds.
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _sut = new UpdateArtistCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator, _mockPathService);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsErrors_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateArtistCommand>()).Returns([Errors.Music.ArtistNameCannotBeEmpty]);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: Guid.NewGuid());
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString());
        StubArtistReads(existingArtist);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryFails_ShouldReturnFailureResultWithoutUpdating()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackPathIsNotWithinLibraryContentLocations_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        // A single content location makes the path check deterministic, so it can be asserted to have run exactly once per track path.
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(contentLocations: ["/music"])));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackPathMustBeWithinLibraryContentLocations, result.FirstError);
        _mockPathService.Received(1).IsPathWithin(Arg.Any<string>(), Arg.Any<string>());
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorRepositoryFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAReferencedContributorDoesNotExist_ShouldReturnMediaContributorNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([]));

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoContributorsAreReferenced_ShouldNotQueryTheContributorRepository()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        AddTrackCommand track = _addTrackCommandFixture.Create(contributors: []);
        AddAlbumCommand album = _addAlbumCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId, contributors: [], tracks: [track]);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), contributors: [], albums: [album]);
        ArtistEntity persistedArtist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, albums: [_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false)], includeContributors: false);
        StubArtistReads(existingArtist, persistedArtist);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExistingArtistDomainConversionFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        ArtistEntity existingArtist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: string.Empty, albums: [album]);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCommandDomainConversionFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), name: string.Empty);
        StubArtistReads(existingArtist);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingTheLastAlbumOfTheArtistFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        // An empty albums list makes the mapping treat the only existing album as stale and remove it, which the aggregate forbids.
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albums: [], contributors: []);
        StubArtistReads(existingArtist);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryUpdateFails_ShouldReturnFailureResultWithoutSavingChanges()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnFailureResultWithoutReadingTheArtist()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedArtistCannotBeRead_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist, error: ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        StubArtistReads(existingArtist, persistedArtist: null, hasPersistedArtist: true);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldUpdateTheArtistAndReturnItsResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ArtistEntity existingArtist = CreateExistingArtist(libraryId, artistId, albumId);
        AddTrackCommand track = _addTrackCommandFixture.Create();
        AddAlbumCommand album = _addAlbumCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId, tracks: [track]);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albums: [album]);
        ArtistEntity persistedArtist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, albums: [_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false)], includeContributors: false);
        ArtistEntity? updatedArtist = null;
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedArtist = callInfo.Arg<ArtistEntity>();
                return Result.Updated;
            });
        StubArtistReads(existingArtist, persistedArtist);

        // Act
        Result<ArtistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(updatedArtist);
        Assert.Equal(artistId, result.Value.Id);
        Assert.Equal(persistedArtist.Name, result.Value.Name);
        await _mockArtistRepository.Received(1).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Creates an existing artist that owns a single album, which belongs to the provided library.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The Id of the artist.</param>
    /// <param name="albumId">The Id of the album the artist owns.</param>
    /// <returns>The created artist entity.</returns>
    private ArtistEntity CreateExistingArtist(Guid libraryId, Guid artistId, Guid albumId)
    {
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false, includeMetadata: true);
        return _artistEntityFixture.Create(id: artistId, libraryId: libraryId, albums: [album], includeContributors: true);
    }

    /// <summary>
    /// Stubs the artist repository so that the initial artist read, and optionally the persisted artist read, return the provided entities.
    /// </summary>
    /// <param name="artist">The artist returned by the initial read.</param>
    /// <param name="persistedArtist">Optional. The artist returned by the persisted read. When not provided, <paramref name="artist"/> is returned again.</param>
    /// <param name="error">Optional. The error returned by the persisted read, when it must fail.</param>
    /// <param name="hasPersistedArtist">Whether the persisted read returns a value, which can be <see langword="null"/>.</param>
    private void StubArtistReads(ArtistEntity artist, ArtistEntity? persistedArtist = null, Error? error = null, bool hasPersistedArtist = false)
    {
        Queue<Result<ArtistEntity?>> readResults = new();
        readResults.Enqueue(Result.From<ArtistEntity?>(artist));
        if (error is not null)
            readResults.Enqueue(error);
        else if (hasPersistedArtist)
            readResults.Enqueue(Result.From(persistedArtist));
        else
            readResults.Enqueue(Result.From<ArtistEntity?>(persistedArtist ?? artist));

        _mockArtistRepository.GetByIdAsync(artist.Id, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(_ => readResults.Dequeue());
    }
}
