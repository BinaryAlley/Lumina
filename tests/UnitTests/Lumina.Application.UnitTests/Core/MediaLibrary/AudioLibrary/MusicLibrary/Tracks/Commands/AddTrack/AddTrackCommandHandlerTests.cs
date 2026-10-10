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
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;

/// <summary>
/// Contains unit tests for the <see cref="AddTrackCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<AddTrackCommand> _mockValidator;
    private readonly IPathService _mockPathService;
    private readonly Guid _userId;
    private readonly AddTrackCommandHandler _sut;
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackCommandHandlerTests"/> class.
    /// </summary>
    public AddTrackCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockValidator = Substitute.For<IValidator<AddTrackCommand>>();
        _userId = Guid.NewGuid();

        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        // Default stub: saving changes succeeds.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        // Default stub: the validator passes and the current user is authenticated.
        _mockValidator.Validate(Arg.Any<AddTrackCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        // Default stub: the library ownership policy allows access.
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        // Default stub: the library exists and the track path is inside its content locations.
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
        // Default stub: the persisted track can be read back by its Id.
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<TrackEntity?>(_trackEntityFixture.Create(id: callInfo.Arg<Guid>())));
        _sut = new AddTrackCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator, _mockPathService);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsErrors_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<AddTrackCommand>())
            .Returns([Errors.Music.TrackPathCannotBeEmpty]);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackPathCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutEvaluatingPolicy()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command, libraryId: Guid.NewGuid());

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // The mismatch is reported as not found, without disclosing that the artist exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        ArrangeArtist(command);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExistInTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command, albumId: Guid.NewGuid());

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackPathIsNotWithinLibraryContentLocations_ShouldReturnFailureResult()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackPathMustBeWithinLibraryContentLocations, result.FirstError);
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorRepositoryFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create()]);
        ArrangeArtist(command);
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAReferencedContributorDoesNotExist_ShouldReturnMediaContributorNotFoundError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create()]);
        ArrangeArtist(command);
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([]));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoContributorsAreReferenced_ShouldNotQueryTheContributorRepository()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(contributors: []);
        ArrangeArtist(command);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDomainConversionFails_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command, name: string.Empty);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));
        ArrangeArtist(command);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackIsAlreadyInTheAlbum_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity existingTrack = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [existingTrack];
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Albums = [album];
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        AddTrackCommand command = _addTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString(), trackId: trackId);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheTrackIsAlreadyInTheAlbum, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryUpdateFails_ShouldReturnFailureResultWithoutSavingChanges()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockArtistRepository.Received(1).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnFailureResultWithoutReadingTheTrack()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedTrackCannotBeRead_ShouldReturnFailureResult()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Music.TrackIdCannotBeEmpty);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackIdCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedTrackDoesNotExist_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        ArrangeArtist(command);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<TrackEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldPersistTheTrackAndReturnItsResponse()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();
        Guid albumId = Guid.Parse(command.AlbumId!);
        Guid libraryId = Guid.Parse(command.LibraryId!);
        ArrangeArtist(command);
        ArtistEntity? updatedArtist = null;
        TrackEntity? persistedTrack = null;
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedArtist = callInfo.Arg<ArtistEntity>();
                return Result.Updated;
            });
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                persistedTrack = _trackEntityFixture.Create(id: callInfo.Arg<Guid>(), albumId: albumId, libraryId: libraryId);
                return Result.From<TrackEntity?>(persistedTrack);
            });

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(persistedTrack);
        Assert.NotNull(updatedArtist);
        Assert.Equal(persistedTrack!.Id, result.Value.Id);
        Assert.Equal(persistedTrack.Path, result.Value.Path);
        Assert.Equal(persistedTrack.Title, result.Value.Metadata.Title);
        Assert.Contains(updatedArtist!.Albums.Single().Tracks, track => track.Id == persistedTrack.Id);
        await _mockArtistRepository.Received(1).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Arranges an existing artist that matches the route identifiers of <paramref name="command"/>, together with one album.
    /// </summary>
    /// <param name="command">The command whose route identifiers are used.</param>
    /// <param name="albumId">Optional. The Id of the album contained by the artist, when it must differ from the route album Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, when it must differ from the route library Id.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <returns>The existing artist entity.</returns>
    private ArtistEntity ArrangeArtist(AddTrackCommand command, Guid? albumId = null, Guid? libraryId = null, string? name = null)
    {
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid resolvedLibraryId = libraryId ?? Guid.Parse(command.LibraryId!);
        Guid resolvedAlbumId = albumId ?? Guid.Parse(command.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: resolvedAlbumId, artistId: artistId, libraryId: resolvedLibraryId, includeTracks: false, includeMetadata: false);
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: resolvedLibraryId, name: name, includeAlbums: false, includeContributors: false);
        artist.Albums = [album];
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        return artist;
    }
}
