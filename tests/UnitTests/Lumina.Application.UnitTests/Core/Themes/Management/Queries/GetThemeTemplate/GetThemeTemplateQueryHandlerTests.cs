#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeTemplate;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeTemplate;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeTemplateQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeService _mockThemeService;
    private readonly IValidator<GetThemeTemplateQuery> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly ILogger<GetThemeTemplateQueryHandler> _mockLogger;
    private readonly GetThemeTemplateQueryHandler _sut;
    private readonly GetThemeTemplateQueryFixture _getThemeTemplateQueryFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateQueryHandlerTests"/> class.
    /// </summary>
    public GetThemeTemplateQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeService = Substitute.For<IThemeService>();
        _mockValidator = Substitute.For<IValidator<GetThemeTemplateQuery>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _mockLogger = Substitute.For<ILogger<GetThemeTemplateQueryHandler>>();

        _mockValidator.Validate(Arg.Any<GetThemeTemplateQuery>())
            .Returns([]);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);

        _sut = new GetThemeTemplateQueryHandler(_mockUnitOfWork, _mockThemeService, _mockValidator, _mockLogger);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutAnyDownstreamCalls()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetThemeTemplateQuery>())
            .Returns([Errors.Themes.ThemeIdCannotBeEmpty]);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeIdCannotBeEmpty, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().GetTemplateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetThemeFails_ShouldReturnError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get theme");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeService.DidNotReceive().GetTemplateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeDoesNotExist_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsDeleted_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockThemeService.DidNotReceive().GetTemplateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTemplateLoadsSuccessfully_ShouldReturnTemplateResponse()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: false);
        string template = "<html>Hello</html>";
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(Result.From(template));

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(theme.Id, result.Value.Theme.Id);
        Assert.Equal(theme.ThemeId, result.Value.Theme.ThemeId);
        Assert.Equal(template, result.Value.Template);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTemplateLoadFailsForUserTheme_ShouldReturnTemplateErrorWithoutRestoring()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        Error error = Error.Failure("Theme.TemplateMissing", "Template missing");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTemplateIsNotFoundForBundledTheme_ShouldNotRestoreBundledTheme()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(Errors.Themes.ThemeTemplateNotFound);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeTemplateNotFound, result.FirstError);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoreThrowsForBundledTheme_ShouldReturnTemplateErrorInsteadOfThrowing()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error templateError = Errors.Themes.ThemeFilesUnreadable;
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError);
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Success>>(new IOException("The theme storage is locked.")));

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(templateError, result.FirstError);
        await _mockThemeService.Received(1).RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTemplateLoadFailsAndRestoreFails_ShouldReturnTemplateError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error templateError = Error.Failure("Theme.TemplateMissing", "Template missing");
        Error restoreError = Error.Failure("Theme.RestoreFailed", "Restore failed");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError);
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(restoreError);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(templateError, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTemplateLoadFailsAndRestoreSucceeds_ShouldRetryAndReturnTemplateResponse()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: false, includeIsCurrent: true, isDeleted: false);
        Error templateError = Error.Failure("Theme.TemplateMissing", "Template missing");
        string restoredTemplate = "<html>Restored</html>";
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError, Result.From(restoredTemplate));
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(restoredTemplate, result.Value.Template);
        await _mockThemeService.Received(2).GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoreSucceedsButRetryFails_ShouldReturnTemplateError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error templateError = Error.Failure("Theme.TemplateMissing", "Template missing");
        Error retryError = Error.Failure("Theme.TemplateMissingAgain", "Template still missing");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError, retryError);
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(retryError, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenRestoringCurrentBundledTheme_ShouldSwitchActiveThemeToDefaultTheme()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        ThemeEntity defaultTheme = _themeEntityFixture.Create(themeId: "default-theme", isDeleted: false);
        Error templateError = Error.Failure("Theme.TemplateMissing", "Template missing");
        string restoredTemplate = "<html>Restored</html>";
        _mockThemeService.DefaultThemeId.Returns(defaultTheme.ThemeId);
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetByThemeIdAsync(defaultTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(defaultTheme));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError, Result.From(restoredTemplate));
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(restoredTemplate, result.Value.Template);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == theme.Id && entity.IsCurrent == null),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(entity => entity.Id == defaultTheme.Id && entity.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoringCurrentBundledThemeWithNoDefaultTheme_ShouldNotSwitchActiveTheme()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        Error templateError = Error.Failure("Theme.TemplateMissing", "Template missing");
        string restoredTemplate = "<html>Restored</html>";
        _mockThemeService.DefaultThemeId.Returns("missing-default");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetByThemeIdAsync("missing-default", Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError, Result.From(restoredTemplate));
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(restoredTemplate, result.Value.Template);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGettingDefaultThemeFails_ShouldNotSwitchActiveThemeButStillRestore()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isCurrent: true, includeIsCurrent: true, isDeleted: false);
        Error templateError = Error.Failure("Theme.TemplateMissing", "Template missing");
        Error defaultError = Error.Failure("Database.Error", "Failed to get default theme");
        string restoredTemplate = "<html>Restored</html>";
        _mockThemeService.DefaultThemeId.Returns("default-theme");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeRepository.GetByThemeIdAsync("default-theme", Arg.Any<CancellationToken>())
            .Returns(defaultError);
        _mockThemeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, Arg.Any<CancellationToken>())
            .Returns(templateError, Result.From(restoredTemplate));
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeTemplateResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(restoredTemplate, result.Value.Template);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
