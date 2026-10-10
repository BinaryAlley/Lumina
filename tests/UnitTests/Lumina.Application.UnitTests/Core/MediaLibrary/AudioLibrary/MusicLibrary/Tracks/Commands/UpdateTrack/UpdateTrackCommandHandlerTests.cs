#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
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
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;

/// <summary>
/// Contains unit tests for the <see cref="UpdateTrackCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateTrackCommand> _mockValidator;
    private readonly IPathService _mockPathService;
    private readonly UpdateTrackCommandHandler _sut;
    private readonly Guid _userId;
    private readonly UpdateTrackCommandFixture _updateTrackCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTrackCommandHandlerTests"/> class.
    /// </summary>
    public UpdateTrackCommandHandlerTests()
    {
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<UpdateTrackCommand>>();
        _mockPathService = Substitute.For<IPathService>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access, the validator passes,
        // the track path is within the library content locations, the artist is updated and the library and contributors exist.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<UpdateTrackCommand>()).Returns([]);
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<TrackEntity?>(_trackEntityFixture.Create(id: callInfo.ArgAt<Guid>(0))));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                IReadOnlyCollection<Guid> contributorIds = callInfo.Arg<IReadOnlyCollection<Guid>>();
                return Result.From<IReadOnlyList<MediaContributorEntity>>([.. contributorIds.Select(contributorId => _mediaContributorEntityFixture.Create(id: contributorId, displayName: contributorId.ToString()))]);
            });

        _sut = new UpdateTrackCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator, _mockPathService);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldUpdateTrackAndReturnResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        ArtistEntity? updatedArtist = null;
        TrackEntity? updatedTrack = null;
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedArtist = callInfo.Arg<ArtistEntity>();
                updatedTrack = updatedArtist.Albums.SelectMany(album => album.Tracks).First(track => track.Id == trackId);
                return Result.Updated;
            });
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<TrackEntity?>(updatedTrack));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(trackId, result.Value.Id);
        Assert.Equal(command.Path, result.Value.Path);
        Assert.Equal(command.Metadata!.Title, result.Value.Metadata!.Title);
        Assert.Equal(command.Contributors!.Count, result.Value.Contributors!.Count);
        await _mockArtistRepository.Received(1).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorFails_ShouldReturnValidationErrorsWithoutUpdating()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateTrackCommand>()).Returns([Errors.Music.TrackIdCannotBeEmpty]);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Music.ArtistNotFound);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        Guid routeLibraryId = Guid.NewGuid();
        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: Guid.NewGuid());
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: routeLibraryId.ToString(), artistId: existingArtist.Id.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedErrorWithoutUpdating()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumIsNotPartOfTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid _, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: Guid.NewGuid().ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackIsNotPartOfTheAlbum_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid _) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: Guid.NewGuid().ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryNotFound);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundErrorWithoutUpdating()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPathIsNotWithinLibraryContentLocations_ShouldReturnPathError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create(contentLocations: ["/music/queen"])));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackPathMustBeWithinLibraryContentLocations, result.FirstError);
        _mockPathService.Received(1).IsPathWithin(command.Path!, Arg.Any<string>());
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoContributorsAreReferenced_ShouldNotQueryTheContributorRepository()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), contributors: []);
        ArtistEntity? updatedArtist = null;
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedArtist = callInfo.Arg<ArtistEntity>();
                return Result.Updated;
            });
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<TrackEntity?>(updatedArtist!.Albums.SelectMany(album => album.Tracks).First(track => track.Id == trackId)));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Errors.MediaContributor.MediaContributorNotFound);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAReferencedContributorDoesNotExist_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), contributors: [_mediaContributorReferenceDtoFixture.Create()]);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([]));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound, result.FirstError);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorIdsAreDuplicated_ShouldQueryTheRepositoryWithDistinctIds()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        Guid firstContributorId = Guid.NewGuid();
        Guid secondContributorId = Guid.NewGuid();
        List<MediaContributorReferenceDto> duplicatedContributors =
        [
            _mediaContributorReferenceDtoFixture.Create(contributorId: firstContributorId, role: MediaContributorRole.Vocals),
            _mediaContributorReferenceDtoFixture.Create(contributorId: firstContributorId, role: MediaContributorRole.Guitar),
            _mediaContributorReferenceDtoFixture.Create(contributorId: secondContributorId, role: MediaContributorRole.Producer),
            _mediaContributorReferenceDtoFixture.Create(contributorId: secondContributorId, role: MediaContributorRole.Producer)
        ];
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), contributors: duplicatedContributors);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaContributorRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(contributorIds => contributorIds.Count == 2 && contributorIds.Contains(firstContributorId) && contributorIds.Contains(secondContributorId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistToDomainEntityConversionFails_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AlbumEntity invalidAlbum = _albumEntityFixture.Create(id: albumId, libraryId: libraryId, tracks: [_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId)]);
        invalidAlbum.Genres = [_genreEntityFixture.Create(name: string.Empty)];
        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: libraryId, albums: [invalidAlbum]);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCommandToDomainEntityConversionFails_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        MusicTrackMetadataDto invalidMetadata = _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), metadata: invalidMetadata);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
        await _mockArtistRepository.DidNotReceive().UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryUpdateFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryNotFound);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnFailureResultWithoutReadingThePersistedTrack()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Errors.Library.LibraryNotFound);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedTrackIsNotFound_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (ArtistEntity existingArtist, Guid albumId, Guid trackId) = CreateArtistWithTrack(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: existingArtist.Id.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    /// <summary>
    /// Creates an artist entity that owns a single album, which in turn owns a single track, all belonging to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <returns>The created artist entity, together with the Ids of its album and track.</returns>
    private (ArtistEntity artist, Guid albumId, Guid trackId) CreateArtistWithTrack(Guid libraryId)
    {
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(
            libraryId: libraryId,
            albums:
            [
                _albumEntityFixture.Create(
                    id: albumId,
                    libraryId: libraryId,
                    tracks: [_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId)])
            ]);
        return (artist, albumId, trackId);
    }
}
