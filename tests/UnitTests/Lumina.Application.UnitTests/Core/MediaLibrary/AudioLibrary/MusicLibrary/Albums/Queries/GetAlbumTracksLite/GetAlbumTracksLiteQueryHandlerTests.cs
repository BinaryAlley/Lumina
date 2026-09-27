#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Contracts.Responses.Common;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;

/// <summary>
/// Contains unit tests for the <see cref="GetAlbumTracksLiteQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksLiteQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetAlbumTracksLiteQuery> _mockValidator;
    private readonly GetAlbumTracksLiteQueryHandler _sut;
    private readonly Guid _userId;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackLiteRowFixture _trackLiteRowFixture = new();
    private readonly GetAlbumTracksLiteQueryFixture _getAlbumTracksLiteQueryFixture = new();
    private readonly PaginatedResultDtoFixture<TrackLiteRow> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksLiteQueryHandlerTests"/> class.
    /// </summary>
    public GetAlbumTracksLiteQueryHandlerTests()
    {
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetAlbumTracksLiteQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access and the validator passes.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetAlbumTracksLiteQuery>()).Returns([]);

        _sut = new GetAlbumTracksLiteQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumExists_ShouldReturnMappedPaginatedTrackLiteResponses()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        List<TrackLiteRow> trackLiteRows = _trackLiteRowFixture.CreateMany(2);
        PaginatedResultDto<TrackLiteRow> paginatedTracks = _paginatedResultDtoFixture.Create(data: trackLiteRows, currentPage: 1, perPage: 10, count: 2, numberOfPages: 1);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetTracksLiteByAlbumIdAsync(albumId, Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedTracks));
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(trackLiteRows.Count, result.Value.Data.Count);
        Assert.Equal(trackLiteRows[0].Id, result.Value.Data[0].Id);
        Assert.Equal(trackLiteRows[0].Title, result.Value.Data[0].Title);
        Assert.Equal(trackLiteRows[0].TrackNumber, result.Value.Data[0].TrackNumber);
        Assert.Equal(paginatedTracks.CurrentPage, result.Value.CurrentPage);
        Assert.Equal(paginatedTracks.PerPage, result.Value.PerPage);
        Assert.Equal(paginatedTracks.Count, result.Value.Count);
        Assert.Equal(paginatedTracks.NumberOfPages, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheAlbumIdAndPaginationToTheRepository()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        PaginatedResultDto<TrackLiteRow> paginatedTracks = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetTracksLiteByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedTracks));
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockTrackRepository.Received(1).GetTracksLiteByAlbumIdAsync(
            albumId,
            Arg.Is<PaginationDataDto>(paginationData => paginationData.CurrentPage == query.PaginationData!.CurrentPage && paginationData.PerPage == query.PaginationData.PerPage),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResultWithoutReadingTracks()
    {
        // Arrange
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetTracksLiteByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(null));

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetTracksLiteByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create();
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: Guid.NewGuid());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetTracksLiteByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: false);
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetTracksLiteByAlbumIdAsync(albumId, Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetAlbumTracksLiteQuery query = _getAlbumTracksLiteQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetAlbumTracksLiteQuery>()).Returns([Errors.Music.AlbumIdCannotBeEmpty]);

        // Act
        Result<PaginatedResponse<TrackLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumIdCannotBeEmpty, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetTracksLiteByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }
}
