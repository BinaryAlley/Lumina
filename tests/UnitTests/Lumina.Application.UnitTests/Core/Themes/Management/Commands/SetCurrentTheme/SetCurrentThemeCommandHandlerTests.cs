#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.SetCurrentTheme;
using Lumina.Contracts.Responses.Themes;
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

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.SetCurrentTheme;

/// <summary>
/// Contains unit tests for the <see cref="SetCurrentThemeCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<SetCurrentThemeCommand> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly SetCurrentThemeCommandHandler _sut;
    private readonly SetCurrentThemeCommandFixture _setCurrentThemeCommandFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeCommandHandlerTests"/> class.
    /// </summary>
    public SetCurrentThemeCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockValidator = Substitute.For<IValidator<SetCurrentThemeCommand>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _userId = Guid.NewGuid();

        _mockValidator.Validate(Arg.Any<SetCurrentThemeCommand>())
            .Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);

        _sut = new SetCurrentThemeCommandHandler(
            _mockUnitOfWork,
            _mockCurrentUserService,
            _mockAuthorizationService,
            _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutAnyDownstreamCalls()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<SetCurrentThemeCommand>())
            .Returns([Errors.Themes.ThemeIdCannotBeEmpty]);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeIdCannotBeEmpty, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetThemeFails_ShouldReturnError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get theme");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeDoesNotExist_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsDeleted_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsAlreadyCurrent_ShouldReturnResponseWithoutAnyPersistence()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(theme.Id, result.Value.Id);
        Assert.Equal(theme.ThemeId, result.Value.ThemeId);
        Assert.True(result.Value.IsCurrent);
        await _mockThemeRepository.DidNotReceive().GetCurrentAsync(Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetCurrentThemeFails_ShouldReturnError()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        Error error = Error.Failure("Database.Error", "Failed to get current theme");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherThemeIsCurrent_ShouldUnsetPreviousThemeAndActivateNewOne()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        ThemeEntity currentTheme = _themeEntityFixture.Create(isCurrent: true, includeIsCurrent: true, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(currentTheme));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(theme.Id, result.Value.Id);
        Assert.True(result.Value.IsCurrent);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == currentTheme.Id && entity.IsCurrent == null),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsCurrent == true && entity.UpdatedBy == _userId),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentThemeIsTheSameTheme_ShouldNotUpdateCurrentThemeTwice()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(theme.Id, result.Value.Id);
        Assert.True(result.Value.IsCurrent);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoThemeIsCurrentlySet_ShouldActivateThemeWithoutUnsettingAnything()
    {
        // Arrange
        SetCurrentThemeCommand command = _setCurrentThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(theme.Id, result.Value.Id);
        Assert.True(result.Value.IsCurrent);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsCurrent == true && entity.UpdatedBy == _userId),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
