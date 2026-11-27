#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;

/// <summary>
/// Contains unit tests for the <see cref="GetDeletedTracksQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedTracksQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetDeletedTracksQuery> _mockValidator;
    private readonly GetDeletedTracksQueryHandler _sut;
    private readonly Guid _userId;
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly GetDeletedTracksQueryFixture _getDeletedTracksQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedTracksQueryHandlerTests"/> class.
    /// </summary>
    public GetDeletedTracksQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetDeletedTracksQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the query is valid, the current user is authenticated, the library ownership policy allows access and there are no deleted tracks.
        _mockValidator.Validate(Arg.Any<GetDeletedTracksQuery>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockTrackRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([]));

        _sut = new GetDeletedTracksQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedTracksExist_ShouldReturnTheirMappedResponses()
    {
        // Arrange
        GetDeletedTracksQuery query = _getDeletedTracksQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        DateTime firstDeletedOnUtc = new(2026, 3, 10, 9, 15, 0, DateTimeKind.Utc);
        AlbumEntity firstAlbum = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false);
        ArtistEntity firstArtist = _artistEntityFixture.Create(id: firstAlbum.ArtistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        firstAlbum.Artist = firstArtist;
        TrackEntity firstTrack = _trackEntityFixture.Create(albumId: firstAlbum.Id, libraryId: libraryId, includeMetadata: false);
        firstTrack.IsDeleted = true;
        firstTrack.DeletedOnUtc = firstDeletedOnUtc;
        firstTrack.Album = firstAlbum;
        AlbumEntity secondAlbum = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false);
        ArtistEntity secondArtist = _artistEntityFixture.Create(id: secondAlbum.ArtistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        secondAlbum.Artist = secondArtist;
        // A deleted track without a recorded deletion time falls back to its creation time, so the history never shows a default date.
        TrackEntity secondTrack = _trackEntityFixture.Create(albumId: secondAlbum.Id, libraryId: libraryId, includeMetadata: false);
        secondTrack.IsDeleted = true;
        secondTrack.DeletedOnUtc = null;
        secondTrack.Album = secondAlbum;
        _mockTrackRepository.GetSoftDeletedByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([firstTrack, secondTrack]));

        // Act
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstTrack.Id, result.Value[0].Id);
        Assert.Equal(firstTrack.LibraryId, result.Value[0].LibraryId);
        Assert.Equal(firstArtist.Id, result.Value[0].ArtistId);
        Assert.Equal(firstArtist.Name, result.Value[0].ArtistName);
        Assert.Equal(firstAlbum.Id, result.Value[0].AlbumId);
        Assert.Equal(firstAlbum.Title, result.Value[0].AlbumTitle);
        Assert.Equal(firstTrack.Title, result.Value[0].Title);
        Assert.Equal(firstTrack.Path, result.Value[0].Path);
        Assert.Equal(firstDeletedOnUtc, result.Value[0].DeletedOnUtc);
        Assert.Equal(secondTrack.Id, result.Value[1].Id);
        Assert.Equal(secondTrack.CreatedOnUtc, result.Value[1].DeletedOnUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoDeletedTracks_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        GetDeletedTracksQuery query = _getDeletedTracksQueryFixture.Create();

        // Act
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        GetDeletedTracksQuery query = _getDeletedTracksQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetDeletedTracksQuery>())
            .Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedTracksQuery query = _getDeletedTracksQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedTracksQuery query = _getDeletedTracksQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        GetDeletedTracksQuery query = _getDeletedTracksQueryFixture.Create();
        _mockTrackRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }
}
