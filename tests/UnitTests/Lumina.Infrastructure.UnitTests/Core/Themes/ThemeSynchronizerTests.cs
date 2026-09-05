#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using Lumina.Infrastructure.Core.Themes;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeSynchronizer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeSynchronizerTests : IDisposable
{
    private readonly IThemeService _mockThemeService;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly ILogger _mockLogger;
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly ThemeManifestDtoFixture _themeManifestDtoFixture = new();
    private readonly string _temporaryDirectoryPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSynchronizerTests"/> class.
    /// </summary>
    public ThemeSynchronizerTests()
    {
        _mockThemeService = Substitute.For<IThemeService>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockLogger = Substitute.For<ILogger>();
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeService.HasThemePack(Arg.Any<string>()).Returns(true);
        _mockThemeService.DefaultThemeId.Returns("lumina-default");
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From(Enumerable.Empty<ThemeEntity>()));
        _mockThemeRepository.InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockThemeRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), $"lumina-theme-sync-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectoryPath);
    }

    [Fact]
    public async Task SynchronizeAsync_WhenTheInstalledThemesCannotBeRead_ShouldLogAndReturn()
    {
        // Arrange
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Themes.NotFound", "Failed to read the installed themes"));
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["bundled.zip"]);

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.DidNotReceive().ReadManifestFromArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenABundledArchiveManifestCannotBeRead_ShouldSkipThatArchive()
    {
        // Arrange
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["missing.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync("missing.zip", Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Manifest", "Failed to read the theme manifest"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenTheBundledThemeIsAlreadyInstalled_ShouldNotReinstallIt()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        ThemeEntity installedTheme = _themeEntityFixture.Create(themeId: manifest.Id, installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { installedTheme });
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["bundled.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync("bundled.zip", Arg.Any<CancellationToken>()).Returns(Result.From(manifest));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenTheBundledThemeWasSoftDeletedByTheUser_ShouldNotReinstallIt()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        ThemeEntity deletedTheme = _themeEntityFixture.Create(themeId: manifest.Id, installSource: ThemeInstallSource.Bundled, isDeleted: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { deletedTheme });
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["bundled.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync("bundled.zip", Arg.Any<CancellationToken>()).Returns(Result.From(manifest));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenABundledThemeIsMissingItsFiles_ShouldReinstallThemWithoutAddingANewRow()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        ThemeEntity damagedTheme = _themeEntityFixture.Create(themeId: manifest.Id, installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { damagedTheme });
        string archivePath = CreateArchiveFile();
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        // The first pack check happens before the repair and reports the files as missing; the later checks report the files as present.
        _mockThemeService.HasThemePack(damagedTheme.ThemeId).Returns(false, true);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(Result.From(manifest));
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(Result.From(manifest));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.Received(1).InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenTheReinstallationFails_ShouldLogAndKeepTheExistingTheme()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        ThemeEntity damagedTheme = _themeEntityFixture.Create(themeId: manifest.Id, installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { damagedTheme });
        string archivePath = CreateArchiveFile();
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.HasThemePack(damagedTheme.ThemeId).Returns(false, true);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(Result.From(manifest));
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Install", "Failed to install the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.Received(1).InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenANewBundledThemeIsInstalled_ShouldInsertTheThemeRow()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        string archivePath = CreateArchiveFile();
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(Result.From(manifest));
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(Result.From(manifest));
        _mockThemeService.HasThemePack(manifest.Id).Returns(true);

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeRepository.Received(1).InsertAsync(Arg.Is<ThemeEntity>(theme => theme.ThemeId == manifest.Id && theme.InstallSource == ThemeInstallSource.Bundled), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenPersistingANewBundledThemeFails_ShouldLogAndContinue()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        string archivePath = CreateArchiveFile();
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(Result.From(manifest));
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(Result.From(manifest));
        _mockThemeService.HasThemePack(manifest.Id).Returns(true);
        _mockThemeRepository.InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Themes.InsertFailed", "Failed to insert the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeRepository.Received(1).InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenTheArchiveFileCannotBeRead_ShouldLogAndContinue()
    {
        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        string missingArchivePath = Path.Combine(_temporaryDirectoryPath, "missing.zip");
        _mockThemeService.GetBundledThemeArchivePaths().Returns([missingArchivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(missingArchivePath, Arg.Any<CancellationToken>()).Returns(Result.From(manifest));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenAccessToTheArchiveFileIsDenied_ShouldLogAndContinue()
    {
        // On non Windows platforms opening a directory behaves differently, so the test only asserts the denied access path on Windows.
        if (!OperatingSystem.IsWindows())
            return;

        // Arrange
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create();
        string deniedArchivePath = _temporaryDirectoryPath;
        _mockThemeService.GetBundledThemeArchivePaths().Returns([deniedArchivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(deniedArchivePath, Arg.Any<CancellationToken>()).Returns(Result.From(manifest));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenAllThemesAreHealthyAndNoArchiveIsBundled_ShouldOnlyPersistTheChanges()
    {
        // Arrange
        ThemeEntity healthyTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { healthyTheme });

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenABundledThemeIsBroken_ShouldRestoreItsFiles()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { brokenTheme });
        _mockThemeService.HasThemePack(brokenTheme.ThemeId).Returns(false);
        _mockThemeService.RestoreBundledThemeAsync(brokenTheme.ThemeId, Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeService.Received(1).RestoreBundledThemeAsync(brokenTheme.ThemeId, Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenTheLastBundledThemeCannotBeRestored_ShouldKeepIt()
    {
        // Arrange
        ThemeEntity lastBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { lastBundledTheme });
        _mockThemeService.HasThemePack(lastBundledTheme.ThemeId).Returns(false);
        _mockThemeService.RestoreBundledThemeAsync(lastBundledTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Restore", "Failed to restore the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        Assert.False(lastBundledTheme.IsDeleted);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenABundledThemeCannotBeRestored_ShouldSoftDeleteIt()
    {
        // Arrange
        ThemeEntity brokenBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity healthyBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { brokenBundledTheme, healthyBundledTheme });
        _mockThemeService.HasThemePack(brokenBundledTheme.ThemeId).Returns(false);
        _mockThemeService.RestoreBundledThemeAsync(brokenBundledTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Restore", "Failed to restore the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        Assert.True(brokenBundledTheme.IsDeleted);
        await _mockThemeRepository.Received(1).UpdateAsync(Arg.Is<ThemeEntity>(theme => theme.Id == brokenBundledTheme.Id), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenSoftDeletingABrokenBundledThemeFails_ShouldLogAndContinue()
    {
        // Arrange
        ThemeEntity brokenBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity healthyBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { brokenBundledTheme, healthyBundledTheme });
        _mockThemeService.HasThemePack(brokenBundledTheme.ThemeId).Returns(false);
        _mockThemeService.RestoreBundledThemeAsync(brokenBundledTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Restore", "Failed to restore the theme"));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Themes.UpdateFailed", "Failed to update the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeRepository.Received(1).UpdateAsync(Arg.Is<ThemeEntity>(theme => theme.Id == brokenBundledTheme.Id), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenAUserThemeIsBroken_ShouldDeleteIt()
    {
        // Arrange
        ThemeEntity brokenUserTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Uploaded, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { brokenUserTheme });
        _mockThemeService.HasThemePack(brokenUserTheme.ThemeId).Returns(false);

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeRepository.Received(1).DeleteByIdAsync(brokenUserTheme.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenDeletingABrokenUserThemeFails_ShouldLogAndContinue()
    {
        // Arrange
        ThemeEntity brokenUserTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Uploaded, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { brokenUserTheme });
        _mockThemeService.HasThemePack(brokenUserTheme.ThemeId).Returns(false);
        _mockThemeRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Themes.DeleteFailed", "Failed to delete the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        await _mockThemeRepository.Received(1).DeleteByIdAsync(brokenUserTheme.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenRepairingOneBrokenThemeThrows_ShouldContinueWithTheRemainingBrokenThemes()
    {
        // Arrange
        ThemeEntity throwingBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity healthyBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { throwingBundledTheme, healthyBundledTheme });
        _mockThemeService.HasThemePack(Arg.Any<string>()).Returns(false);
        _mockThemeService.RestoreBundledThemeAsync(throwingBundledTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Success>>(new InvalidOperationException("Unexpected failure")));
        _mockThemeService.RestoreBundledThemeAsync(healthyBundledTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Restore", "Failed to restore the theme"));

        // Act
        await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);

        // Assert
        // The second broken bundled theme is still soft deleted even though the first one threw.
        Assert.True(healthyBundledTheme.IsDeleted);
        await _mockThemeRepository.Received(1).UpdateAsync(Arg.Is<ThemeEntity>(theme => theme.Id == healthyBundledTheme.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAsync_WhenRepairingABrokenThemeIsCancelled_ShouldRethrowTheCancellation()
    {
        // Arrange
        ThemeEntity brokenBundledTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { brokenBundledTheme });
        _mockThemeService.HasThemePack(brokenBundledTheme.ThemeId).Returns(false);
        _mockThemeService.RestoreBundledThemeAsync(brokenBundledTheme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Success>>(new OperationCanceledException()));

        // Act
        async Task Act()
        {
            await ThemeSynchronizer.SynchronizeAsync(_mockThemeService, _mockUnitOfWork, _mockLogger, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(Act);
    }

    [Fact]
    public async Task EnsureCurrentThemeExistsAsync_WhenANonDeletedThemeIsCurrent_ShouldNotSelectAnotherTheme()
    {
        // Arrange
        ThemeEntity currentTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: false, includeIsCurrent: true, isCurrent: true);
        ThemeEntity otherTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Uploaded, isDeleted: false);

        // Act
        await InvokeEnsureCurrentThemeExistsAsync([currentTheme, otherTheme]);

        // Assert
        Assert.True(currentTheme.IsCurrent);
        Assert.NotEqual(true, otherTheme.IsCurrent);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureCurrentThemeExistsAsync_WhenNoThemeCanBeSelected_ShouldNotSelectAnyTheme()
    {
        // Arrange
        ThemeEntity deletedTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Bundled, isDeleted: true);

        // Act
        await InvokeEnsureCurrentThemeExistsAsync([deletedTheme]);

        // Assert
        Assert.NotEqual(true, deletedTheme.IsCurrent);
        await _mockThemeRepository.DidNotReceive().UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureCurrentThemeExistsAsync_WhenNoThemeIsCurrent_ShouldActivateTheDefaultTheme()
    {
        // Arrange
        ThemeEntity defaultTheme = _themeEntityFixture.Create(themeId: "lumina-default", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity userTheme = _themeEntityFixture.Create(installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        _mockThemeService.HasThemePack(defaultTheme.ThemeId).Returns(true);
        _mockThemeService.HasThemePack(userTheme.ThemeId).Returns(false);

        // Act
        await InvokeEnsureCurrentThemeExistsAsync([userTheme, defaultTheme]);

        // Assert
        Assert.True(defaultTheme.IsCurrent);
        await _mockThemeRepository.Received(1).UpdateAsync(Arg.Is<ThemeEntity>(theme => theme.Id == defaultTheme.Id), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Creates a real archive file on disk so that <see cref="ThemeSynchronizer"/> can open it.
    /// </summary>
    /// <returns>The path of the created archive file.</returns>
    private string CreateArchiveFile()
    {
        string archivePath = Path.Combine(_temporaryDirectoryPath, $"theme-{Guid.NewGuid():N}.zip");
        File.WriteAllText(archivePath, "theme-pack");
        return archivePath;
    }

    /// <summary>
    /// Invokes the private <c>EnsureCurrentThemeExistsAsync</c> method of the synchronizer.
    /// </summary>
    /// <param name="themes">The installed themes passed to the method.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task InvokeEnsureCurrentThemeExistsAsync(IReadOnlyList<ThemeEntity> themes)
    {
        MethodInfo method = typeof(ThemeSynchronizer).GetMethod("EnsureCurrentThemeExistsAsync", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The method 'EnsureCurrentThemeExistsAsync' was not found on the theme synchronizer.");
        Task task = (Task)method.Invoke(null, [_mockThemeService, _mockUnitOfWork, themes, CancellationToken.None])!;
        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the temporary files created by the tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectoryPath))
            Directory.Delete(_temporaryDirectoryPath, recursive: true);
    }
}
