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
using Lumina.Infrastructure.Fixtures.Core.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeDetectionSyncJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeDetectionSyncJobTests : IDisposable
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly IThemeService _mockThemeService;
    private readonly ILogger<ThemeDetectionSyncJob> _mockLogger;
    private readonly ThemePackFixture _themePackFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly ThemeManifestDtoFixture _themeManifestDtoFixture = new();
    private readonly string _testRootPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeDetectionSyncJobTests"/> class.
    /// </summary>
    public ThemeDetectionSyncJobTests()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), $"lumina-theme-job-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRootPath);
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateAsyncScope().Returns(new AsyncServiceScope(_mockServiceScope));
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockThemeService = Substitute.For<IThemeService>();
        _mockThemeService.HasThemePack(Arg.Any<string>()).Returns(true);
        _mockLogger = Substitute.For<ILogger<ThemeDetectionSyncJob>>();
    }

    /// <summary>
    /// Cleans up the temporary storage used by the tests.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, recursive: true);
        }
        catch (IOException)
        {
            // best effort cleanup of the per-test temp directory
        }
        catch (UnauthorizedAccessException)
        {
            // best effort cleanup of the per-test temp directory
        }
    }

    [Fact]
    public async Task StartAsync_WhenThereAreNoBundledThemesAndNoInstalledThemes_ShouldOnlySaveChanges()
    {
        // Arrange
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().InsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenBundledThemeIsNotYetInstalled_ShouldInstallAndPersistTheTheme()
    {
        // Arrange
        string archivePath = CreateBundledArchiveFile("bundled-theme");
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create(id: "bundled-theme");
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(manifest);
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(manifest);
        _mockThemeRepository.InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.From(Result.Created));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.Received(1).InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).InsertAsync(
            Arg.Is<ThemeEntity>(theme => theme.ThemeId == "bundled-theme"
                && theme.Name == manifest.Name
                && theme.Description == manifest.Description
                && theme.Version == manifest.Version
                && theme.InstallSource == ThemeInstallSource.Bundled),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenBundledThemeIsAlreadyInstalled_ShouldSkipInstallingIt()
    {
        // Arrange
        ThemeEntity existingTheme = _themeEntityFixture.Create(themeId: "bundled-theme", isCurrent: true, includeIsCurrent: true, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([existingTheme]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["C:\\bundled\\bundled-theme.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync("C:\\bundled\\bundled-theme.zip", Arg.Any<CancellationToken>()).Returns(_themeManifestDtoFixture.Create(id: "bundled-theme"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().InsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenBundledThemeWasSoftDeletedByTheUser_ShouldNotReinstallIt()
    {
        // Arrange
        ThemeEntity deletedTheme = _themeEntityFixture.Create(themeId: "bundled-theme", isDeleted: true, installSource: ThemeInstallSource.Bundled);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([deletedTheme]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["C:\\bundled\\bundled-theme.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync("C:\\bundled\\bundled-theme.zip", Arg.Any<CancellationToken>()).Returns(_themeManifestDtoFixture.Create(id: "bundled-theme"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().InsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenInstalledBundledThemePackIsMissingAndNotDeleted_ShouldReinstallTheFilesWithoutInsertingANewRow()
    {
        // Arrange
        ThemeEntity existingTheme = _themeEntityFixture.Create(themeId: "bundled-theme", isCurrent: true, includeIsCurrent: true, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([existingTheme]));
        _mockThemeService.HasThemePack("bundled-theme").Returns(false);
        string archivePath = CreateBundledArchiveFile("bundled-theme");
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create(id: "bundled-theme");
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(manifest);
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(manifest);
        _mockThemeService.RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.Received(1).InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().InsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenBundledArchiveManifestCannotBeRead_ShouldSkipTheArchive()
    {
        // Arrange
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["C:\\bundled\\broken.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync("C:\\bundled\\broken.zip", Arg.Any<CancellationToken>()).Returns(Error.Failure("Theme.Files", "unreadable"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.DidNotReceive().InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().InsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenBundledThemeInstallFails_ShouldNotPersistTheTheme()
    {
        // Arrange
        string archivePath = CreateBundledArchiveFile("bundled-theme");
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(_themeManifestDtoFixture.Create(id: "bundled-theme"));
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(Error.Failure("Theme.Install", "install failed"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().InsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenPersistingDetectedThemeFails_ShouldLogWarningAndContinue()
    {
        // Arrange
        string archivePath = CreateBundledArchiveFile("bundled-theme");
        ThemeManifestDto manifest = _themeManifestDtoFixture.Create(id: "bundled-theme");
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns([archivePath]);
        _mockThemeService.ReadManifestFromArchiveAsync(archivePath, Arg.Any<CancellationToken>()).Returns(manifest);
        _mockThemeService.InstallAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(manifest);
        _mockThemeRepository.InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Error.Failure("Database.Error", "failed to persist"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).InsertAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenReadingInstalledThemesFails_ShouldReturnWithoutPersistingChanges()
    {
        // Arrange
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Error.Failure("Database.Error", "failed to read themes"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        _mockThemeService.DidNotReceive().GetBundledThemeArchivePaths();
        await _mockUnitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenMissingBundledThemePackCanBeRestored_ShouldRestoreTheFiles()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(themeId: "bundled-theme", isCurrent: true, includeIsCurrent: true, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([brokenTheme]));
        _mockThemeService.HasThemePack("bundled-theme").Returns(false);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeService.RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.Received(1).RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>());
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenRestoringBundledThemeThrows_ShouldNotCrashTheJob()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(themeId: "bundled-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([brokenTheme]));
        _mockThemeService.HasThemePack("bundled-theme").Returns(false);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeService.RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Success>>(new IOException("The theme storage is locked.")));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.From(Result.Updated));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeService.Received(1).RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenLastBundledThemePackCannotBeRestored_ShouldKeepAndActivateTheTheme()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(themeId: "bundled-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([brokenTheme]));
        _mockThemeService.HasThemePack("bundled-theme").Returns(false);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeService.RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>()).Returns(Error.Failure("Theme.Files", "restore failed"));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.From(Result.Updated));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(theme => theme.ThemeId == "bundled-theme" && theme.IsCurrent == true && !theme.IsDeleted),
            Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).RestoreBundledThemeAsync("bundled-theme", Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenMissingBundledThemePackCannotBeRestoredAndAnotherBundledThemeRemains_ShouldSoftDeleteTheTheme()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(themeId: "broken-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity healthyTheme = _themeEntityFixture.Create(themeId: "healthy-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([brokenTheme, healthyTheme]));
        _mockThemeService.HasThemePack("broken-theme").Returns(false);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeService.RestoreBundledThemeAsync("broken-theme", Arg.Any<CancellationToken>()).Returns(Error.Failure("Theme.Files", "restore failed"));
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.From(Result.Updated));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(theme => theme.ThemeId == "broken-theme" && theme.IsDeleted && theme.IsCurrent == null),
            Arg.Any<CancellationToken>());
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(theme => theme.ThemeId == "healthy-theme" && theme.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenMissingUserThemePackExists_ShouldDeleteTheTheme()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(themeId: "user-theme", installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([brokenTheme]));
        _mockThemeService.HasThemePack("user-theme").Returns(false);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeRepository.DeleteByIdAsync(brokenTheme.Id, Arg.Any<CancellationToken>()).Returns(Result.From(Result.Deleted));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).DeleteByIdAsync(brokenTheme.Id, Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenDeletingMissingUserThemeFails_ShouldLogWarningAndContinue()
    {
        // Arrange
        ThemeEntity brokenTheme = _themeEntityFixture.Create(themeId: "user-theme", installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([brokenTheme]));
        _mockThemeService.HasThemePack("user-theme").Returns(false);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeRepository.DeleteByIdAsync(brokenTheme.Id, Arg.Any<CancellationToken>()).Returns(Error.Failure("Database.Error", "delete failed"));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).DeleteByIdAsync(brokenTheme.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenNoThemeIsCurrent_ShouldActivateTheDefaultTheme()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create(themeId: "default-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([theme]));
        _mockThemeService.DefaultThemeId.Returns("default-theme");
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.From(Result.Updated));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(candidate => candidate.ThemeId == "default-theme" && candidate.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenNoThemeIsCurrent_ShouldPreferTheConfiguredDefaultTheme()
    {
        // Arrange
        ThemeEntity firstTheme = _themeEntityFixture.Create(themeId: "alpha-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity defaultTheme = _themeEntityFixture.Create(themeId: "default-theme", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([firstTheme, defaultTheme]));
        _mockThemeService.DefaultThemeId.Returns("default-theme");
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeRepository.UpdateAsync(Arg.Any<ThemeEntity>(), Arg.Any<CancellationToken>()).Returns(Result.From(Result.Updated));

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.Received(1).UpdateAsync(
            Arg.Is<ThemeEntity>(candidate => candidate.ThemeId == "default-theme" && candidate.IsCurrent == true),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenAThemeIsAlreadyCurrent_ShouldNotChangeActivation()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create(themeId: "current-theme", isCurrent: true, includeIsCurrent: true, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ThemeEntity>>([theme]));
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);

        // Act
        await StartAndWaitForExecutionAsync(CreateSut());

        // Assert
        await _mockThemeRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Invokes the protected <see cref="BackgroundService.ExecuteAsync"/> of the job and awaits its completion.
    /// </summary>
    /// <param name="sut">The job to execute.</param>
    private static async Task StartAndWaitForExecutionAsync(ThemeDetectionSyncJob sut)
    {
        MethodInfo executeAsyncMethod = typeof(ThemeDetectionSyncJob).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Task executingTask = (Task)executeAsyncMethod.Invoke(sut, [CancellationToken.None])!;
        await executingTask;
    }

    /// <summary>
    /// Creates the system under test wired to the mocked dependencies.
    /// </summary>
    /// <returns>The created job instance.</returns>
    private ThemeDetectionSyncJob CreateSut()
    {
        return new ThemeDetectionSyncJob(_mockServiceScopeFactory, _mockThemeService, _mockLogger);
    }

    /// <summary>
    /// Writes a bundled theme archive file into the per-test temporary directory.
    /// </summary>
    /// <param name="themeId">The theme id of the bundled archive.</param>
    /// <returns>The path of the written archive.</returns>
    private string CreateBundledArchiveFile(string themeId)
    {
        string bundledPath = Path.Combine(_testRootPath, "bundled");
        Directory.CreateDirectory(bundledPath);
        string archivePath = Path.Combine(bundledPath, $"{themeId}.zip");
        File.WriteAllBytes(archivePath, _themePackFixture.Create(themeId: themeId));
        return archivePath;
    }
}
