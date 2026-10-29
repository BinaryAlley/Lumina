#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.UsersManagement.Settings.Queries.GetPlaybackState;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Queries.GetPlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackStateQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly GetPlaybackStateQueryHandler _sut;
    private readonly GetPlaybackStateQueryFixture _getPlaybackStateQueryFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateQueryHandlerTests"/> class.
    /// </summary>
    public GetPlaybackStateQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();

        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockCurrentUserService.UserId.Returns(_userId);

        _sut = new GetPlaybackStateQueryHandler(_mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedWithoutReadingSettings()
    {
        // Arrange
        GetPlaybackStateQuery query = _getPlaybackStateQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaybackStateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByUserIdReturnsError_ShouldReturnError()
    {
        // Arrange
        GetPlaybackStateQuery query = _getPlaybackStateQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to retrieve user settings");
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaybackStateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsDoNotExist_ShouldReturnTheDefaultPlaybackState()
    {
        // Arrange
        GetPlaybackStateQuery query = _getPlaybackStateQueryFixture.Create();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));

        // Act
        Result<PlaybackStateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1.0, result.Value.Volume);
        Assert.False(result.Value.IsMuted);
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsExist_ShouldReturnTheStoredPlaybackState()
    {
        // Arrange
        GetPlaybackStateQuery query = _getPlaybackStateQueryFixture.Create();
        UserSettingsEntity storedSettings = _userSettingsEntityFixture.Create(userId: _userId, volume: 0.42, isMuted: true);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(storedSettings));

        // Act
        Result<PlaybackStateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0.42, result.Value.Volume);
        Assert.True(result.Value.IsMuted);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCurrentUserIdAndCancellationToken()
    {
        // Arrange
        GetPlaybackStateQuery query = _getPlaybackStateQueryFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockUserSettingsRepository.Received(1).GetByUserIdAsync(_userId, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequested_ShouldCancelTheOperation()
    {
        // Arrange
        GetPlaybackStateQuery query = _getPlaybackStateQueryFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<Result<UserSettingsEntity?>>(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.HandleAsync(query, cancellationTokenSource.Token));
        await _mockUserSettingsRepository.Received(1).GetByUserIdAsync(_userId, cancellationTokenSource.Token);
    }
}
