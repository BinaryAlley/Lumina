#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.RestoreTheme;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.RestoreTheme;

/// <summary>
/// Contains unit tests for the <see cref="RestoreThemeCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeService _mockThemeService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<RestoreThemeCommand> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly RestoreThemeCommandHandler _sut;
    private readonly RestoreThemeCommandFixture _restoreThemeCommandFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeCommandHandlerTests"/> class.
    /// </summary>
    public RestoreThemeCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeService = Substitute.For<IThemeService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockValidator = Substitute.For<IValidator<RestoreThemeCommand>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _userId = Guid.NewGuid();

        _mockValidator.Validate(Arg.Any<RestoreThemeCommand>())
            .Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _mockThemeService.RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new RestoreThemeCommandHandler(
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
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<RestoreThemeCommand>())
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
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
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
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
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
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsNotDeleted_ShouldReturnThemeCannotBeRestoredError()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeCannotBeRestored, result.FirstError);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsDeletedButNotBundled_ShouldReturnThemeCannotBeRestoredError()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Uploaded, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeCannotBeRestored, result.FirstError);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenFileRestorationFails_ShouldReturnErrorWithoutReactivatingTheme()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: true);
        Error error = Error.Failure("Theme.FilesUnreadable", "Failed to restore theme files");
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoringDeletedBundledTheme_ShouldRestoreFilesAndReactivateTheme()
    {
        // Arrange
        RestoreThemeCommand command = _restoreThemeCommandFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: command.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(command.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeService.Received(1).RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && !entity.IsDeleted && entity.IsCurrent == null && entity.UpdatedBy == _userId),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
