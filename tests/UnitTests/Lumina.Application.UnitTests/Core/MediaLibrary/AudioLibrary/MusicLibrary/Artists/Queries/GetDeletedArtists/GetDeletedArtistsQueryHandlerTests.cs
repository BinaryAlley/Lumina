#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;

/// <summary>
/// Contains unit tests for the <see cref="GetDeletedArtistsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedArtistsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetDeletedArtistsQuery> _mockValidator;
    private readonly GetDeletedArtistsQueryHandler _sut;
    private readonly Guid _userId;
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly GetDeletedArtistsQueryFixture _getDeletedArtistsQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedArtistsQueryHandlerTests"/> class.
    /// </summary>
    public GetDeletedArtistsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetDeletedArtistsQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the query is valid, the current user is authenticated, the library ownership policy allows access, there are no deleted
        // artists and none of them has stored artwork.
        _mockValidator.Validate(Arg.Any<GetDeletedArtistsQuery>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockArtistRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ArtistEntity>>([]));
        _mockMusicArtworkRepository.GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));

        _sut = new GetDeletedArtistsQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedArtistsExist_ShouldReturnTheirMappedResponsesWithCoverPaths()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        string coverFileName = $"covers/{Guid.NewGuid()}.jpg";
        DateTime firstDeletedOnUtc = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        ArtistEntity firstArtist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        firstArtist.IsDeleted = true;
        firstArtist.DeletedOnUtc = firstDeletedOnUtc;
        // A deleted artist without a recorded deletion time falls back to its creation time, so the history never shows a default date.
        ArtistEntity secondArtist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        secondArtist.IsDeleted = true;
        secondArtist.DeletedOnUtc = null;
        _mockArtistRepository.GetSoftDeletedByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ArtistEntity>>([firstArtist, secondArtist]));
        _mockMusicArtworkRepository.GetByOwnersAsync(MusicArtworkOwnerType.Artist, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(
            [
                // A cover row whose stored file is not resolved yet is skipped, so it cannot hide the resolved cover of the same artist.
                _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: firstArtist.Id, artworkType: ArtworkType.Cover, ordinal: 1, includeFileName: false),
                _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: firstArtist.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: coverFileName),
                // Non cover artwork is not used as the cover of the deleted artist.
                _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: firstArtist.Id, artworkType: ArtworkType.Back, ordinal: 0)
            ]));

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstArtist.Id, result.Value[0].Id);
        Assert.Equal(firstArtist.LibraryId, result.Value[0].LibraryId);
        Assert.Equal(firstArtist.Name, result.Value[0].Name);
        Assert.Equal(firstDeletedOnUtc, result.Value[0].DeletedOnUtc);
        Assert.Equal(coverFileName, result.Value[0].CoverPath);
        Assert.Equal(secondArtist.Id, result.Value[1].Id);
        Assert.Equal(secondArtist.CreatedOnUtc, result.Value[1].DeletedOnUtc);
        Assert.Null(result.Value[1].CoverPath);
        await _mockMusicArtworkRepository.Received(1).GetByOwnersAsync(
            MusicArtworkOwnerType.Artist,
            Arg.Is<IReadOnlyCollection<Guid>>(ownerIds => ownerIds.Count == 2 && ownerIds.Contains(firstArtist.Id) && ownerIds.Contains(secondArtist.Id)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoDeletedArtists_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetDeletedArtistsQuery>())
            .Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();
        _mockArtistRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        GetDeletedArtistsQuery query = _getDeletedArtistsQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.IsDeleted = true;
        artist.DeletedOnUtc = DateTime.UtcNow;
        _mockArtistRepository.GetSoftDeletedByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ArtistEntity>>([artist]));
        _mockMusicArtworkRepository.GetByOwnersAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }
}
