#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Core.Playlists.Queries.GetPlaylists;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Queries.GetPlaylists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Queries.GetPlaylists;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly GetPlaylistsQueryHandler _sut;
    private readonly GetPlaylistsQueryFixture _getPlaylistsQueryFixture = new();
    private readonly PlaylistSummaryDtoFixture _playlistSummaryDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsQueryHandlerTests"/> class.
    /// </summary>
    public GetPlaylistsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockPlaylistRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<PlaylistSummaryDto>>([_playlistSummaryDtoFixture.Create()]));

        _sut = new GetPlaylistsQueryHandler(_mockUnitOfWork, _mockCurrentUserService);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheRepository()
    {
        // Arrange
        GetPlaylistsQuery query = _getPlaylistsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistSummaryResponse[]> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRepositoryFails_ShouldReturnThatError()
    {
        // Arrange
        GetPlaylistsQuery query = _getPlaylistsQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlists could not be read.");
        _mockPlaylistRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistSummaryResponse[]> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserHasNoPlaylists_ShouldReturnAnEmptyArray()
    {
        // Arrange
        GetPlaylistsQuery query = _getPlaylistsQueryFixture.Create();
        _mockPlaylistRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<PlaylistSummaryDto>>([]));

        // Act
        Result<PlaylistSummaryResponse[]> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldMapEveryPlaylistSummaryInOrder()
    {
        // Arrange
        DateTime firstCreatedOnUtc = new(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        DateTime secondCreatedOnUtc = new(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc);
        DateTime secondUpdatedOnUtc = new(2024, 7, 8, 9, 10, 11, DateTimeKind.Utc);
        PlaylistSummaryDto firstPlaylist = _playlistSummaryDtoFixture.Create(
            name: "Focus",
            description: "Deep work",
            type: PlaylistType.Music,
            entryCount: 12,
            createdOnUtc: firstCreatedOnUtc,
            includeUpdatedOnUtc: false);
        PlaylistSummaryDto secondPlaylist = _playlistSummaryDtoFixture.Create(
            name: "Commute",
            includeDescription: false,
            type: PlaylistType.Music,
            entryCount: 3,
            createdOnUtc: secondCreatedOnUtc,
            includeUpdatedOnUtc: true,
            updatedOnUtc: secondUpdatedOnUtc);
        _mockPlaylistRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<PlaylistSummaryDto>>([firstPlaylist, secondPlaylist]));
        GetPlaylistsQuery query = _getPlaylistsQueryFixture.Create();

        // Act
        Result<PlaylistSummaryResponse[]> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Length);
        Assert.Equal(firstPlaylist.Id, result.Value[0].Id);
        Assert.Equal("Focus", result.Value[0].Name);
        Assert.Equal("Deep work", result.Value[0].Description);
        Assert.Equal(PlaylistType.Music, result.Value[0].Type);
        Assert.Equal(12, result.Value[0].EntryCount);
        Assert.Equal(firstCreatedOnUtc, result.Value[0].CreatedOnUtc);
        Assert.Null(result.Value[0].UpdatedOnUtc);
        Assert.Equal(secondPlaylist.Id, result.Value[1].Id);
        Assert.Equal("Commute", result.Value[1].Name);
        Assert.Null(result.Value[1].Description);
        Assert.Equal(3, result.Value[1].EntryCount);
        Assert.Equal(secondCreatedOnUtc, result.Value[1].CreatedOnUtc);
        Assert.True(result.Value[1].UpdatedOnUtc.HasValue);
        Assert.Equal(secondUpdatedOnUtc, result.Value[1].UpdatedOnUtc!.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldReadThePlaylistsOfTheCurrentUserAndForwardTheCancellationToken()
    {
        // Arrange
        GetPlaylistsQuery query = _getPlaylistsQueryFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetAllForUserAsync(_userId, Arg.Is(cancellationToken));
    }
}
