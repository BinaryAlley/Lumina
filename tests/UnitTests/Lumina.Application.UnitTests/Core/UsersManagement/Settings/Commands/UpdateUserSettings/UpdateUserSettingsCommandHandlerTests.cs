#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IValidator<UpdateUserSettingsCommand> _mockValidator;
    private readonly UpdateUserSettingsCommandHandler _sut;
    private readonly UpdateUserSettingsCommandFixture _updateUserSettingsCommandFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsCommandHandlerTests"/> class.
    /// </summary>
    public UpdateUserSettingsCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();

        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockCurrentUserService.UserId.Returns(_userId);

        _mockValidator = Substitute.For<IValidator<UpdateUserSettingsCommand>>();
        _mockValidator.Validate(Arg.Any<UpdateUserSettingsCommand>())
            .Returns([]);

        _sut = new UpdateUserSettingsCommandHandler(_mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsDoNotExist_ShouldInsertSettingsAndReturnUpdated()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();
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
                settings.IsPaginationEnabled == command.IsPaginationEnabled &&
                settings.ItemsPerPage == command.ItemsPerPage &&
                settings.IgnoreThePrefixForAlphaPicker == command.IgnoreThePrefixForAlphaPicker),
            Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsExist_ShouldUpdateSettingsAndReturnUpdated()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();
        UserSettingsEntity existingSettings = _userSettingsEntityFixture.Create(userId: _userId);
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
                settings.IsPaginationEnabled == command.IsPaginationEnabled &&
                settings.ItemsPerPage == command.ItemsPerPage &&
                settings.IgnoreThePrefixForAlphaPicker == command.IgnoreThePrefixForAlphaPicker),
            Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotPersist()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateUserSettingsCommand>())
            .Returns([DomainErrors.UserSettings.ItemsPerPageMustBeGreaterThanZero]);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.UserSettings.ItemsPerPageMustBeGreaterThanZero, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().InsertAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUserSettingsRepository.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotPersist()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();
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
    public async Task HandleAsync_WhenGetByUserIdReturnsError_ShouldReturnError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();
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
}
