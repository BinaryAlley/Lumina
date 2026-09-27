#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;

/// <summary>
/// Contains unit tests for the <see cref="GetAlbumTracksQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetAlbumTracksQuery> _mockValidator;
    private readonly GetAlbumTracksQueryHandler _sut;
    private readonly Guid _userId;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly GetAlbumTracksQueryFixture _getAlbumTracksQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksQueryHandlerTests"/> class.
    /// </summary>
    public GetAlbumTracksQueryHandlerTests()
    {
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetAlbumTracksQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access and the validator passes.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetAlbumTracksQuery>()).Returns([]);

        _sut = new GetAlbumTracksQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumExists_ShouldReturnMappedTracks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        List<TrackEntity> tracks = _trackEntityFixture.CreateMany(2);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>(tracks));
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(tracks.Count, result.Value.Count);
        Assert.Equal(tracks[0].Id, result.Value[0].Id);
        Assert.Equal(tracks[1].Id, result.Value[1].Id);
        await _mockTrackRepository.Received(1).GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResultWithoutReadingTracks()
    {
        // Arrange
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(null));

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create();
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: Guid.NewGuid());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The mismatch is reported as not found, without disclosing that the album exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The mismatch is reported as not found, without disclosing that the album exists under another artist.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetAlbumTracksQuery query = _getAlbumTracksQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetAlbumTracksQuery>()).Returns([Errors.Music.AlbumIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<TrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumIdCannotBeEmpty, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
