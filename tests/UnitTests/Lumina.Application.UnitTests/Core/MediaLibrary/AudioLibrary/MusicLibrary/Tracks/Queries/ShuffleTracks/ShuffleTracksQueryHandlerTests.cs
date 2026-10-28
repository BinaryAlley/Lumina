#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;

/// <summary>
/// Contains unit tests for the <see cref="ShuffleTracksQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<ShuffleTracksQuery> _mockValidator;
    private readonly ShuffleTracksQueryHandler _sut;
    private readonly Guid _userId;
    private readonly ShuffleTracksQueryFixture _shuffleTracksQueryFixture = new();
    private readonly ShuffleTrackRowFixture _shuffleTrackRowFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksQueryHandlerTests"/> class.
    /// </summary>
    public ShuffleTracksQueryHandlerTests()
    {
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<ShuffleTracksQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<ShuffleTracksQuery>()).Returns([]);

        _sut = new ShuffleTracksQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsTracks_ShouldReturnMappedResponses()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        Guid firstAlbumId = Guid.NewGuid();
        Guid secondAlbumId = Guid.NewGuid();
        Guid firstArtistId = Guid.NewGuid();
        Guid secondArtistId = Guid.NewGuid();
        List<ShuffleTrackRowDto> rows =
        [
            _shuffleTrackRowFixture.Create(
                trackId: firstTrackId,
                title: "Bohemian Rhapsody",
                durationInSeconds: 355,
                albumId: firstAlbumId,
                albumTitle: "A Night at the Opera",
                artistId: firstArtistId,
                coverPath: "a-night-at-the-opera-cover.jpg"),
            _shuffleTrackRowFixture.Create(
                trackId: secondTrackId,
                title: "Love of My Life",
                durationInSeconds: 219,
                albumId: secondAlbumId,
                albumTitle: "A Night at the Opera",
                artistId: secondArtistId,
                includeCoverPath: false)
        ];
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackRowDto>>(rows));

        // Act
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstTrackId, result.Value[0].TrackId);
        Assert.Equal("Bohemian Rhapsody", result.Value[0].Title);
        Assert.Equal(355, result.Value[0].DurationInSeconds);
        Assert.Equal(firstAlbumId, result.Value[0].AlbumId);
        Assert.Equal("A Night at the Opera", result.Value[0].AlbumTitle);
        Assert.Equal(firstArtistId, result.Value[0].ArtistId);
        Assert.Equal("a-night-at-the-opera-cover.jpg", result.Value[0].CoverPath);
        Assert.Equal(secondTrackId, result.Value[1].TrackId);
        Assert.Equal("Love of My Life", result.Value[1].Title);
        Assert.Equal(219, result.Value[1].DurationInSeconds);
        Assert.Equal(secondAlbumId, result.Value[1].AlbumId);
        Assert.Equal("A Night at the Opera", result.Value[1].AlbumTitle);
        Assert.Equal(secondArtistId, result.Value[1].ArtistId);
        Assert.Null(result.Value[1].CoverPath);
    }

    [Fact]
    public async Task HandleAsync_WhenTracksAreRequested_ShouldQueryRepositoryWithParsedLibraryScopeAndCount()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(
            libraryId: libraryId.ToString(),
            count: 25,
            artistId: artistId,
            albumId: albumId,
            excludedTrackIds: [Guid.NewGuid(), Guid.NewGuid()]);
        CancellationToken cancellationToken = new();
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackRowDto>>([]));

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockTrackRepository.Received(1).GetShuffleTracksLiteAsync(
            libraryId,
            artistId,
            albumId,
            Arg.Is<IReadOnlyCollection<Guid>>(excludedIds => excludedIds.Count == 2),
            query.Count,
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenExcludedTrackIdsAreNull_ShouldQueryRepositoryWithAnEmptyCollection()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(includeExcludedTrackIds: false);
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackRowDto>>([]));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockTrackRepository.Received(1).GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Is<IReadOnlyCollection<Guid>>(excludedIds => excludedIds.Count == 0),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExcludedTrackIdsContainEmptyAndRepeatedIds_ShouldQueryRepositoryWithTheDistinctNonEmptyIds()
    {
        // Arrange
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(
            excludedTrackIds: [firstTrackId, Guid.Empty, secondTrackId, firstTrackId]);
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackRowDto>>([]));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockTrackRepository.Received(1).GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Is<IReadOnlyCollection<Guid>>(excludedIds =>
                excludedIds.Count == 2 &&
                excludedIds.Contains(firstTrackId) &&
                excludedIds.Contains(secondTrackId) &&
                !excludedIds.Contains(Guid.Empty)),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsEvaluated_ShouldUseTheParsedLibraryIdAndTheCurrentUser()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(libraryId: libraryId.ToString());
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackRowDto>>([]));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<ShuffleTracksQuery>()).Returns([Errors.Music.ShuffleTrackCountMustBeGreaterThanZero]);

        // Act
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ShuffleTrackCountMustBeGreaterThanZero, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(libraryId: libraryId.ToString());
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId),
            Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create();
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsNoTracks_ShouldReturnAnEmptyList()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create();
        _mockTrackRepository.GetShuffleTracksLiteAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackRowDto>>([]));

        // Act
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
