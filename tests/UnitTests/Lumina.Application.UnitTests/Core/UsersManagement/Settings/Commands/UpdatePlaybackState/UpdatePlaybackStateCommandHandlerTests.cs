#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaybackStateCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IValidator<UpdatePlaybackStateCommand> _mockValidator;
    private readonly UpdatePlaybackStateCommandHandler _sut;
    private readonly UpdatePlaybackStateCommandFixture _updatePlaybackStateCommandFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateCommandHandlerTests"/> class.
    /// </summary>
    public UpdatePlaybackStateCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();

        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockCurrentUserService.UserId.Returns(_userId);

        _mockValidator = Substitute.For<IValidator<UpdatePlaybackStateCommand>>();
        _mockValidator.Validate(Arg.Any<UpdatePlaybackStateCommand>())
            .Returns([]);

        _sut = new UpdatePlaybackStateCommandHandler(_mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsAndNotPersist()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdatePlaybackStateCommand>())
            .Returns([DomainErrors.UserSettings.VolumeMustBeBetweenZeroAndOne]);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.UserSettings.VolumeMustBeBetweenZeroAndOne, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotPersist()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByUserIdReturnsError_ShouldReturnErrorAndNotPersist()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to retrieve user settings");
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsDoNotExist_ShouldInsertTheReportedStateKeepingTheHandEditedSettingsAtTheirDefaults()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(volume: 0.35, isMuted: true);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));
        _mockUserSettingsRepository.InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        await _mockUserSettingsRepository.Received(1).InsertAsync(
            Arg.Is<UserSettingsEntity>(settings =>
                settings.UserId == _userId &&
                settings.Volume == 0.35 &&
                settings.IsMuted &&
                settings.IsPaginationEnabled &&
                settings.ItemsPerPage == 48 &&
                settings.ShouldAutoplay &&
                settings.IsGaplessPlaybackEnabled &&
                settings.VuMeterSegmentCount == 12),
            Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));
        Error error = Error.Failure("Database.Error", "Failed to insert user settings");
        _mockUserSettingsRepository.InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsExist_ShouldUpdateTheReportedStateAndPreserveTheHandEditedSettings()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(volume: 0.25, isMuted: true);
        UserSettingsEntity existingSettings = _userSettingsEntityFixture.Create(
            userId: _userId,
            volume: 1.0,
            isMuted: false,
            itemsPerPage: 24,
            isPaginationEnabled: false);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(existingSettings));
        _mockUserSettingsRepository.UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        await _mockUserSettingsRepository.Received(1).UpdateAsync(
            Arg.Is<UserSettingsEntity>(settings =>
                settings.UserId == _userId &&
                settings.Volume == 0.25 &&
                settings.IsMuted &&
                !settings.IsPaginationEnabled &&
                settings.ItemsPerPage == 24),
            Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        UserSettingsEntity existingSettings = _userSettingsEntityFixture.Create(userId: _userId);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(existingSettings));
        Error error = Error.Failure("Database.Error", "Failed to update user settings");
        _mockUserSettingsRepository.UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFailsAfterInsert_ShouldReturnError()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));
        _mockUserSettingsRepository.InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        Error error = Error.Failure("Database.Error", "Failed to save changes");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFailsAfterUpdate_ShouldReturnError()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        UserSettingsEntity existingSettings = _userSettingsEntityFixture.Create(userId: _userId);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(existingSettings));
        _mockUserSettingsRepository.UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        Error error = Error.Failure("Database.Error", "Failed to save changes");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCurrentUserIdAndCancellationToken()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));
        _mockUserSettingsRepository.InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockUserSettingsRepository.Received(1).GetByUserIdAsync(_userId, Arg.Is(cancellationToken));
        await _mockUserSettingsRepository.Received(1).InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequested_ShouldCancelTheOperation()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<Result<UserSettingsEntity?>>(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.HandleAsync(command, cancellationTokenSource.Token));
        await _mockUserSettingsRepository.Received(1).GetByUserIdAsync(_userId, cancellationTokenSource.Token);
    }
}
