#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.DeleteTheme;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.DeleteTheme;

/// <summary>
/// Contains unit tests for the <see cref="DeleteThemeCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeService _mockThemeService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<DeleteThemeCommand> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly DeleteThemeCommandHandler _sut;
    private readonly DeleteThemeCommandFixture _deleteThemeCommandFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeCommandHandlerTests"/> class.
    /// </summary>
    public DeleteThemeCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeService = Substitute.For<IThemeService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockValidator = Substitute.For<IValidator<DeleteThemeCommand>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _userId = Guid.NewGuid();

        _mockValidator.Validate(Arg.Any<DeleteThemeCommand>())
            .Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _mockThemeService.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new DeleteThemeCommandHandler(
            _mockUnitOfWork,
            _mockThemeService,
            _mockCurrentUserService,
            _mockAuthorizationService,
            _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutAnyDownstreamCalls()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteThemeCommand>())
            .Returns([Errors.Themes.ThemeIdCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

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
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

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
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetThemeFails_ShouldReturnError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get theme");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeDoesNotExist_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsAlreadyDeleted_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllThemesFails_ShouldReturnError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        Error error = Error.Failure("Database.Error", "Failed to get all themes");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingLastBundledTheme_ShouldReturnLastBundledThemeCannotBeDeletedError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity userTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, userTheme]));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.LastBundledThemeCannotBeDeleted, result.FirstError);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingCurrentThemeWithoutReplacement_ShouldReturnThemeCannotBeDeletedError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Uploaded, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme]));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeCannotBeDeleted, result.FirstError);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingBundledTheme_ShouldSoftDeleteItAndRemoveStoredFiles()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        ThemeEntity otherBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, otherBundledTheme]));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsDeleted && entity.IsCurrent == null && entity.UpdatedBy == _userId),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).DeleteAsync(theme.ThemeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingUserTheme_ShouldHardDeleteItAndRemoveStoredFiles()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Uploaded, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        ThemeEntity bundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, bundledTheme]));
        _mockThemeRepository.DeleteByIdAsync(theme.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeRepository.Received(1).DeleteByIdAsync(theme.Id, Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).DeleteAsync(theme.ThemeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenHardDeleteFails_ShouldReturnErrorWithoutRemovingStoredFiles()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Uploaded, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        ThemeEntity bundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error error = Error.Failure("Database.Error", "Failed to delete theme");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, bundledTheme]));
        _mockThemeRepository.DeleteByIdAsync(theme.Id, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingCurrentBundledTheme_ShouldSwitchToDefaultReplacementThemeAndSoftDelete()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        ThemeEntity replacementTheme = _themeEntityFixture.Create(themeId: "default-theme", installSource: ThemeInstallSource.Bundled, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        _mockThemeService.DefaultThemeId.Returns(replacementTheme.ThemeId);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, replacementTheme]));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == replacementTheme.Id && entity.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsDeleted && entity.IsCurrent == null),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).DeleteAsync(theme.ThemeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoDefaultThemeMatches_ShouldSwitchToAlphabeticallyFirstReplacementTheme()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        ThemeEntity alphaTheme = _themeEntityFixture.Create(themeId: "alpha", installSource: ThemeInstallSource.Bundled, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        ThemeEntity betaTheme = _themeEntityFixture.Create(themeId: "beta", installSource: ThemeInstallSource.Bundled, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        _mockThemeService.DefaultThemeId.Returns("no-such-default");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, betaTheme, alphaTheme]));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == alphaTheme.Id && entity.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsDeleted),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenFileDeletionFails_ShouldReturnError()
    {
        // Arrange
        DeleteThemeCommand command = _deleteThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Uploaded, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        ThemeEntity bundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error error = Error.Failure("Theme.DeleteFailed", "Failed to delete theme files");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([theme, bundledTheme]));
        _mockThemeRepository.DeleteByIdAsync(theme.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockThemeService.DeleteAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
