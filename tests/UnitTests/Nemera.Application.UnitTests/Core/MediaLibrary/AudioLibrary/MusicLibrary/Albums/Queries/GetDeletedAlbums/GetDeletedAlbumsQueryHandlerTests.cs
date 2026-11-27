#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;

/// <summary>
/// Contains unit tests for the <see cref="GetDeletedAlbumsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedAlbumsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetDeletedAlbumsQuery> _mockValidator;
    private readonly GetDeletedAlbumsQueryHandler _sut;
    private readonly Guid _userId;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly GetDeletedAlbumsQueryFixture _getDeletedAlbumsQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedAlbumsQueryHandlerTests"/> class.
    /// </summary>
    public GetDeletedAlbumsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetDeletedAlbumsQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the query is valid, the current user is authenticated, the library ownership policy allows access, there are no deleted
        // albums and none of them has stored artwork.
        _mockValidator.Validate(Arg.Any<GetDeletedAlbumsQuery>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockAlbumRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([]));
        _mockMusicArtworkRepository.GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));

        _sut = new GetDeletedAlbumsQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedAlbumsExist_ShouldReturnTheirMappedResponsesWithCoverPaths()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        string coverFileName = $"covers/{Guid.NewGuid()}.jpg";
        DateTime firstDeletedOnUtc = new(2026, 2, 20, 18, 45, 0, DateTimeKind.Utc);
        AlbumEntity firstAlbum = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false);
        firstAlbum.IsDeleted = true;
        firstAlbum.DeletedOnUtc = firstDeletedOnUtc;
        firstAlbum.Artist = _artistEntityFixture.Create(id: firstAlbum.ArtistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        // A deleted album without a recorded deletion time falls back to its creation time, so the history never shows a default date.
        AlbumEntity secondAlbum = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false);
        secondAlbum.IsDeleted = true;
        secondAlbum.DeletedOnUtc = null;
        secondAlbum.Artist = _artistEntityFixture.Create(id: secondAlbum.ArtistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        _mockAlbumRepository.GetSoftDeletedByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([firstAlbum, secondAlbum]));
        _mockMusicArtworkRepository.GetByOwnersAsync(MusicArtworkOwnerType.Album, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(
            [
                // A cover row whose stored file is not resolved yet is skipped, so it cannot hide the resolved cover of the same album.
                _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: firstAlbum.Id, artworkType: ArtworkType.Cover, ordinal: 1, includeFileName: false),
                _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: firstAlbum.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: coverFileName),
                // Non cover artwork is not used as the cover of the deleted album.
                _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: firstAlbum.Id, artworkType: ArtworkType.Booklet, ordinal: 0)
            ]));

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstAlbum.Id, result.Value[0].Id);
        Assert.Equal(firstAlbum.LibraryId, result.Value[0].LibraryId);
        Assert.Equal(firstAlbum.ArtistId, result.Value[0].ArtistId);
        Assert.Equal(firstAlbum.Artist.Name, result.Value[0].ArtistName);
        Assert.Equal(firstAlbum.Title, result.Value[0].Title);
        Assert.Equal(firstDeletedOnUtc, result.Value[0].DeletedOnUtc);
        Assert.Equal(coverFileName, result.Value[0].CoverPath);
        Assert.Equal(secondAlbum.Id, result.Value[1].Id);
        Assert.Equal(secondAlbum.Artist.Name, result.Value[1].ArtistName);
        Assert.Equal(secondAlbum.CreatedOnUtc, result.Value[1].DeletedOnUtc);
        Assert.Null(result.Value[1].CoverPath);
        await _mockMusicArtworkRepository.Received(1).GetByOwnersAsync(
            MusicArtworkOwnerType.Album,
            Arg.Is<IReadOnlyCollection<Guid>>(ownerIds => ownerIds.Count == 2 && ownerIds.Contains(firstAlbum.Id) && ownerIds.Contains(secondAlbum.Id)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoDeletedAlbums_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetDeletedAlbumsQuery>())
            .Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();
        _mockAlbumRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        GetDeletedAlbumsQuery query = _getDeletedAlbumsQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false);
        album.IsDeleted = true;
        album.DeletedOnUtc = DateTime.UtcNow;
        _mockAlbumRepository.GetSoftDeletedByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([album]));
        _mockMusicArtworkRepository.GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }
}
