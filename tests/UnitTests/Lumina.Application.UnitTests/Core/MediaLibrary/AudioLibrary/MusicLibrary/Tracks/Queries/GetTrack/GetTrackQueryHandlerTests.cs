#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetTrackQuery> _mockValidator;
    private readonly GetTrackQueryHandler _sut;
    private readonly Guid _userId;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly GetTrackQueryFixture _getTrackQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackQueryHandlerTests"/> class.
    /// </summary>
    public GetTrackQueryHandlerTests()
    {
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetTrackQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetTrackQuery>()).Returns([]);

        _sut = new GetTrackQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenTrackExists_ShouldReturnMappedTrackResponse()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        TrackEntity trackEntity = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", title: "Bohemian Rhapsody");
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));
        _mockTrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(trackEntity));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(trackId, result.Value.Id);
        Assert.Equal(albumId, result.Value.AlbumId);
        Assert.Equal(libraryId, result.Value.LibraryId);
        Assert.Equal("/music/queen/bohemian-rhapsody.flac", result.Value.Path);
        Assert.Equal("Bohemian Rhapsody", result.Value.Metadata.Title);
        Assert.Equal(trackEntity.TrackNumber, result.Value.TrackNumber);
        Assert.NotNull(result.Value.Moods);
        Assert.NotEmpty(result.Value.Moods!);
        Assert.NotNull(result.Value.Isrcs);
        Assert.NotEmpty(result.Value.Isrcs!);
        Assert.NotNull(result.Value.Contributors);
        Assert.NotEmpty(result.Value.Contributors!);
        Assert.NotNull(result.Value.Ratings);
        Assert.NotEmpty(result.Value.Ratings!);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == trackEntity.LibraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumNotFound_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid albumId = Guid.Parse(query.AlbumId!);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The route artist id is enforced before the track is read, and the mismatch is reported as not found, without disclosing that the album exists under another artist.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: Guid.NewGuid());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The route library id is enforced before the track is read, and the mismatch is reported as not found, without disclosing that the album exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid albumId = Guid.Parse(query.AlbumId!);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackNotFound_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));
        _mockTrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackBelongsToAnotherAlbum_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        TrackEntity trackEntity = _trackEntityFixture.Create(id: trackId, albumId: Guid.NewGuid(), libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));
        _mockTrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(trackEntity));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The route album id is enforced, so a track of another album can never be read through this album's route.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackBelongsToAnotherLibrary_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        TrackEntity trackEntity = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: Guid.NewGuid());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));
        _mockTrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(trackEntity));

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The route library id is enforced, so a track of another library can never be read through this library's route.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));
        _mockTrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Music.TrackIdCannotBeEmpty);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        TrackEntity trackEntity = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(albumEntity));
        _mockTrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(trackEntity));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == trackEntity.LibraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetTrackQuery>()).Returns([Errors.Music.TrackIdCannotBeEmpty]);

        // Act
        Result<TrackResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackIdCannotBeEmpty, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }
}
