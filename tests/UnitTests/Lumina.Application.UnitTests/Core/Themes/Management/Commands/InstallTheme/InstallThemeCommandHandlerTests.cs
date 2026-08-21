#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Commands.InstallTheme;

/// <summary>
/// Contains unit tests for the <see cref="InstallThemeCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeService _mockThemeService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<InstallThemeCommand> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly InstallThemeCommandHandler _sut;
    private readonly InstallThemeCommandFixture _installThemeCommandFixture = new();
    private readonly ThemeManifestDtoFixture _themeManifestDtoFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeCommandHandlerTests"/> class.
    /// </summary>
    public InstallThemeCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeService = Substitute.For<IThemeService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockValidator = Substitute.For<IValidator<InstallThemeCommand>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _userId = Guid.NewGuid();

        _mockValidator.Validate(Arg.Any<InstallThemeCommand>())
            .Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);

        _sut = new InstallThemeCommandHandler(
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
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<InstallThemeCommand>())
            .Returns([Errors.Themes.ThemeArchiveCannotBeNull]);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeArchiveCannotBeNull, result.FirstError);
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeInstallFails_ShouldReturnError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        Error error = Error.Failure("Theme.InstallFailed", "Failed to install theme");
        _mockThemeService.InstallAsync(command.Archive!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetThemeFails_ShouldReturnError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get theme");
        _mockThemeService.InstallAsync(command.Archive!, Arg.Any<CancellationToken>())
            .Returns(Result.From(manifest));
        _mockThemeRepository.GetByThemeIdAsync(manifest.Id, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeDoesNotExist_ShouldInsertNewUploadedTheme()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        _mockThemeService.InstallAsync(command.Archive!, Arg.Any<CancellationToken>())
            .Returns(Result.From(manifest));
        _mockThemeRepository.GetByThemeIdAsync(manifest.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));
        _mockThemeRepository.InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(manifest.Id, result.Value.ThemeId);
        Assert.Equal(manifest.Name, result.Value.Name);
        Assert.Equal(manifest.Description, result.Value.Description);
        Assert.Equal(manifest.Author, result.Value.Author);
        Assert.Equal(manifest.Version, result.Value.Version);
        Assert.Equal(manifest.Preview, result.Value.PreviewPath);
        Assert.Equal(ThemeInstallSource.Uploaded, result.Value.InstallSource);
        Assert.Null(result.Value.IsCurrent);
        await _mockThemeRepository.Received(1).InsertAsync(
            Arg.Is<ThemeEntity>(theme =>
                theme.ThemeId == manifest.Id &&
                theme.Name == manifest.Name &&
                theme.Description == manifest.Description &&
                theme.Author == manifest.Author &&
                theme.Version == manifest.Version &&
                theme.PreviewPath == manifest.Preview &&
                theme.InstallSource == ThemeInstallSource.Uploaded &&
                theme.IsDeleted == false &&
                theme.CreatedBy == _userId &&
                theme.UpdatedBy == _userId),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldRollBackStoredThemeFiles()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to insert theme");
        _mockThemeService.InstallAsync(command.Archive!, Arg.Any<CancellationToken>())
            .Returns(Result.From(manifest));
        _mockThemeRepository.GetByThemeIdAsync(manifest.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));
        _mockThemeRepository.InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeService.Received(1).DeleteAsync(manifest.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeAlreadyExists_ShouldUpdateExistingThemeAndResurrectIt()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        ThemeEntity existingTheme = _themeEntityFixture.Create(
            themeId: manifest.Id,
            installSource: ThemeInstallSource.Bundled,
            isCurrent: true, includeIsCurrent: true,
            isDeleted: true);
        _mockThemeService.InstallAsync(command.Archive!, Arg.Any<CancellationToken>())
            .Returns(Result.From(manifest));
        _mockThemeRepository.GetByThemeIdAsync(manifest.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(existingTheme));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(existingTheme.Id, result.Value.Id);
        Assert.Equal(manifest.Name, result.Value.Name);
        Assert.Equal(ThemeInstallSource.Bundled, result.Value.InstallSource);
        Assert.True(result.Value.IsCurrent);
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(theme =>
                theme.Id == existingTheme.Id &&
                theme.ThemeId == manifest.Id &&
                theme.Name == manifest.Name &&
                theme.InstallSource == existingTheme.InstallSource &&
                theme.IsCurrent == existingTheme.IsCurrent &&
                theme.IsDeleted == false &&
                theme.CreatedOnUtc == existingTheme.CreatedOnUtc &&
                theme.CreatedBy == existingTheme.CreatedBy &&
                theme.UpdatedBy == _userId),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnError()
    {
        // Arrange
        InstallThemeCommand command = _installThemeCommandFixture.Create();
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        ThemeEntity existingTheme = _themeEntityFixture.Create(themeId: manifest.Id, installSource: ThemeInstallSource.Bundled, isCurrent: null, isDeleted: false);
        Error error = Error.Failure("Database.Error", "Failed to update theme");
        _mockThemeService.InstallAsync(command.Archive!, Arg.Any<CancellationToken>())
            .Returns(Result.From(manifest));
        _mockThemeRepository.GetByThemeIdAsync(manifest.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(existingTheme));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
