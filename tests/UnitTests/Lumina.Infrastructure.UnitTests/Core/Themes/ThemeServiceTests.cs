#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Core.Themes;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Fixtures.Core.Themes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeServiceTests : IDisposable
{
    private const string DEFAULT_TEMPLATE = "<html><body>default template</body></html>";
    private const string HOME_TEMPLATE = "<html><body>home template</body></html>";
    private const string LIBRARY_TEMPLATE = "<html><body>library template</body></html>";

    private readonly string _testRootPath;
    private readonly ThemeEngineOptionsDto _options;
    private readonly ThemeService _sut;
    private readonly ThemePackFixture _themePackFixture = new();
    private readonly ThemeEngineOptionsDtoFixture _themeEngineOptionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeServiceTests"/> class.
    /// </summary>
    public ThemeServiceTests()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), $"lumina-themes-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRootPath);
        _options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        _sut = CreateSut(_options);
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
    public void AllowThemeScripts_WhenConfiguredFalse_ShouldReturnFalse()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.AllowThemeScripts = false;
        ThemeService sut = CreateSut(options);

        // Act
        bool result = sut.AllowThemeScripts;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AllowThemeScripts_WhenConfiguredTrue_ShouldReturnTrue()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.AllowThemeScripts = true;
        ThemeService sut = CreateSut(options);

        // Act
        bool result = sut.AllowThemeScripts;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MaxArchiveBytes_WhenConfigured_ShouldReturnConfiguredValue()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.MaxArchiveBytes = 12345;
        ThemeService sut = CreateSut(options);

        // Act
        long result = sut.MaxArchiveBytes;

        // Assert
        Assert.Equal(12345, result);
    }

    [Fact]
    public void DefaultThemeId_WhenConfigured_ShouldReturnConfiguredValue()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.DefaultThemeId = "custom-default";
        ThemeService sut = CreateSut(options);

        // Act
        string result = sut.DefaultThemeId;

        // Assert
        Assert.Equal("custom-default", result);
    }

    [Fact]
    public void GetBundledThemeArchivePaths_WhenBundledDirectoryDoesNotExist_ShouldReturnEmptyList()
    {
        // Arrange
        // the bundled directory of the default options never exists

        // Act
        IReadOnlyList<string> result = _sut.GetBundledThemeArchivePaths();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetBundledThemeArchivePaths_WhenBundledDirectoryContainsZips_ShouldReturnSortedZipPaths()
    {
        // Arrange
        string bundledPath = Path.Combine(_testRootPath, "bundled");
        Directory.CreateDirectory(bundledPath);
        File.WriteAllBytes(Path.Combine(bundledPath, "zeta.zip"), _themePackFixture.Create());
        File.WriteAllBytes(Path.Combine(bundledPath, "alpha.zip"), _themePackFixture.Create());
        File.WriteAllBytes(Path.Combine(bundledPath, "notes.txt"), Encoding.UTF8.GetBytes("not a theme archive"));

        // Act
        IReadOnlyList<string> result = _sut.GetBundledThemeArchivePaths();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(Path.Combine(bundledPath, "alpha.zip"), result[0]);
        Assert.Equal(Path.Combine(bundledPath, "zeta.zip"), result[1]);
    }

    [Fact]
    public async Task HasThemePack_WhenThemeIsInstalled_ShouldReturnTrue()
    {
        // Arrange
        await InstallValidThemeAsync(_themePackFixture.Create());

        // Act
        bool result = _sut.HasThemePack("test-theme");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasThemePack_WhenThemeIsNotInstalled_ShouldReturnFalse()
    {
        // Arrange
        // nothing is installed

        // Act
        bool result = _sut.HasThemePack("test-theme");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasThemePack_WhenThemeDirectoryExistsButManifestIsMissing_ShouldReturnFalse()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_options.StoragePath, "test-theme"));

        // Act
        bool result = _sut.HasThemePack("test-theme");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveIsValid_ShouldExtractThemeAndReturnManifest()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(defaultTemplateContent: DEFAULT_TEMPLATE);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-theme", result.Value.Id);
        Assert.Equal("Test Theme", result.Value.Name);
        Assert.True(File.Exists(Path.Combine(_options.StoragePath, "test-theme", "theme.json")));
        Assert.True(File.Exists(Path.Combine(_options.StoragePath, "test-theme", "templates", "default.html")));
        Assert.True(File.Exists(Path.Combine(_options.StoragePath, "test-theme", "assets", "preview.png")));
    }

    [Fact]
    public async Task InstallAsync_WhenThemeIsAlreadyInstalled_ShouldReplaceExistingThemeFiles()
    {
        // Arrange
        byte[] firstArchive = _themePackFixture.Create(defaultTemplateContent: "first version");
        byte[] secondArchive = _themePackFixture.Create(defaultTemplateContent: "second version");

        // Act
        await InstallValidThemeAsync(firstArchive);
        await InstallValidThemeAsync(secondArchive);
        Result<string> templateResult = await _sut.GetTemplateAsync("test-theme", "unknown-page", CancellationToken.None);

        // Assert
        Assert.True(templateResult.IsSuccess);
        Assert.Equal("second version", templateResult.Value);
        string[] themeDirectories = [.. Directory.GetDirectories(_options.StoragePath).Where(path => Path.GetFileName(path) != ".staging")];
        Assert.Single(themeDirectories);
        Assert.Equal("test-theme", Path.GetFileName(themeDirectories[0]));
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveExceedsMaxArchiveBytes_ShouldReturnArchiveTooLargeError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.MaxArchiveBytes = 128;
        ThemeService sut = CreateSut(options);
        byte[] archive = _themePackFixture.Create();

        // Act
        Result<ThemeManifestDto> result = await sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Archive.TooLarge", result.FirstError.Code);
        Assert.False(sut.HasThemePack("test-theme"));
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveStreamIsEmpty_ShouldReturnPackageInvalidError()
    {
        // Arrange
        using MemoryStream emptyStream = new();

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(emptyStream, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("empty", result.FirstError.Description);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveContainsNoEntries_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.CreateArchive();

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("no files", result.FirstError.Description);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveIsNotAZip_ShouldReturnThemeArchiveNotReadable()
    {
        // Arrange
        byte[] archive = Encoding.UTF8.GetBytes("this is not a valid zip archive");

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeArchiveNotReadable", result.FirstError.Description);
        Assert.False(_sut.HasThemePack("test-theme"));
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveContainsPathTraversalEntry_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson();
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("templates/default.html", DEFAULT_TEMPLATE),
            new("../evil.txt", "should not escape the package"));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.False(_sut.HasThemePack("test-theme"));
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveContainsBackslashPathEntry_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson();
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("templates/default.html", DEFAULT_TEMPLATE),
            new("assets\\..\\evil.txt", "should not escape the package"));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveEntryIsOutsideAllowedLocations_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson();
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("templates/default.html", DEFAULT_TEMPLATE),
            new("notes/readme.txt", "files must live under templates or assets"));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("outside the allowed", result.FirstError.Description);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveContainsSymbolicLinkEntry_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson();
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("templates/default.html", DEFAULT_TEMPLATE),
            new("assets/link.png", "symlink target", UnixFileType: 0xA000));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("Symbolic links", result.FirstError.Description);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveContainsDuplicateEntryPaths_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson();
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("templates/default.html", DEFAULT_TEMPLATE),
            new("templates/default.html", "duplicate"));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("duplicate path", result.FirstError.Description);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveExceedsMaxEntries_ShouldReturnTooManyEntriesError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.MaxEntries = 2;
        ThemeService sut = CreateSut(options);
        byte[] archive = _themePackFixture.Create();

        // Act
        Result<ThemeManifestDto> result = await sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Archive.TooManyEntries", result.FirstError.Code);
    }

    [Fact]
    public async Task InstallAsync_WhenArchiveFileExceedsMaxSingleFileBytes_ShouldReturnFileTooLargeError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.MaxSingleFileBytes = 10;
        ThemeService sut = CreateSut(options);
        string manifestJson = _themePackFixture.CreateManifestJson();
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("templates/default.html", new string('a', 64)));

        // Act
        Result<ThemeManifestDto> result = await sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.File.TooLarge", result.FirstError.Code);
    }

    [Fact]
    public async Task InstallAsync_WhenExpandedSizeExceedsMaxExpandedBytes_ShouldReturnExpandedTooLargeError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.MaxSingleFileBytes = 400;
        options.MaxExpandedBytes = 300;
        ThemeService sut = CreateSut(options);
        byte[] archive = _themePackFixture.CreateArchive(
            new("templates/default.html", new string('a', 200)),
            new("templates/other.html", new string('b', 200)));

        // Act
        Result<ThemeManifestDto> result = await sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Archive.ExpandedTooLarge", result.FirstError.Code);
    }

    [Fact]
    public async Task InstallAsync_WhenSchemaVersionIsUnsupported_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(schemaVersion: 2);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "schemaVersion");
    }

    [Fact]
    public async Task InstallAsync_WhenThemeIdIsNotKebabCase_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(themeId: "Test_Theme");

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "kebab-case");
    }

    [Fact]
    public async Task InstallAsync_WhenThemeIdIsTooLong_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string longThemeId = string.Join('-', Enumerable.Repeat("a", 33));
        byte[] archive = _themePackFixture.Create(themeId: longThemeId);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "kebab-case");
    }

    [Fact]
    public async Task InstallAsync_WhenNameIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(name: string.Empty);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "name");
    }

    [Fact]
    public async Task InstallAsync_WhenDescriptionIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(description: string.Empty);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "description");
    }

    [Fact]
    public async Task InstallAsync_WhenAuthorIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(author: string.Empty);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "author");
    }

    [Fact]
    public async Task InstallAsync_WhenVersionIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(version: string.Empty);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "version");
    }

    [Fact]
    public async Task InstallAsync_WhenVersionIsNotSemanticVersion_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(version: "1.0");

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "semantic version");
    }

    [Fact]
    public async Task InstallAsync_WhenManifestDeclaresNoTemplates_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(templates: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "between 1 and 32");
    }

    [Fact]
    public async Task InstallAsync_WhenManifestDeclaresTooManyTemplates_ShouldReturnPackageInvalidError()
    {
        // Arrange
        Dictionary<string, string> manyTemplates = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < 33; index++)
            manyTemplates[$"template-{index}"] = $"templates/template-{index}.html";
        byte[] archive = _themePackFixture.Create(templates: manyTemplates);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "between 1 and 32");
    }

    [Fact]
    public async Task InstallAsync_WhenTemplateKeyIsInvalid_ShouldReturnPackageInvalidError()
    {
        // Arrange
        Dictionary<string, string> templates = new(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = "templates/default.html",
            ["Bad Key"] = "templates/bad-key.html"
        };
        byte[] archive = _themePackFixture.Create(templates: templates);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "Template key");
    }

    [Fact]
    public async Task InstallAsync_WhenTemplatePathIsOutsideTemplatesDirectory_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson(templates: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = "assets/not-a-template.html"
        });
        byte[] archive = _themePackFixture.CreateArchive(
            new("theme.json", manifestJson),
            new("assets/not-a-template.html", DEFAULT_TEMPLATE));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "under templates");
    }

    [Fact]
    public async Task InstallAsync_WhenTemplateFileIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string manifestJson = _themePackFixture.CreateManifestJson(templates: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = "templates/missing.html"
        });
        byte[] archive = _themePackFixture.CreateArchive(new ThemeArchiveEntry("theme.json", manifestJson));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "missing file");
    }

    [Fact]
    public async Task InstallAsync_WhenManifestLacksDefaultTemplate_ShouldReturnPackageInvalidError()
    {
        // Arrange
        Dictionary<string, string> templates = new(StringComparer.OrdinalIgnoreCase)
        {
            ["home"] = "templates/home.html"
        };
        byte[] archive = _themePackFixture.Create(templates: templates);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "default");
    }

    [Fact]
    public async Task InstallAsync_WhenPreviewIsOutsideAssetsDirectory_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(preview: "templates/default.html");

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "preview");
    }

    [Fact]
    public async Task InstallAsync_WhenPreviewFileIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(preview: "assets/missing.png", includePreviewAsset: false);

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        AssertPackageInvalid(result, "preview");
    }

    [Fact]
    public async Task InstallAsync_WhenManifestIsNotValidJson_ShouldReturnInvalidJsonError()
    {
        // Arrange
        byte[] archive = _themePackFixture.CreateArchive(new ThemeArchiveEntry("theme.json", "this is not json"));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Manifest.InvalidJson", result.FirstError.Code);
    }

    [Fact]
    public async Task InstallAsync_WhenManifestExceedsMaximumSize_ShouldReturnManifestTooLargeError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(description: new string('a', 70 * 1024));

        // Act
        Result<ThemeManifestDto> result = await _sut.InstallAsync(new MemoryStream(archive), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Manifest.TooLarge", result.FirstError.Code);
    }

    [Fact]
    public async Task ReadManifestFromArchiveAsync_WhenArchiveIsValid_ShouldReturnManifest()
    {
        // Arrange
        string archivePath = Path.Combine(_testRootPath, "valid-theme.zip");
        File.WriteAllBytes(archivePath, _themePackFixture.Create(themeId: "manifest-theme"));

        // Act
        Result<ThemeManifestDto> result = await _sut.ReadManifestFromArchiveAsync(archivePath, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("manifest-theme", result.Value.Id);
    }

    [Fact]
    public async Task ReadManifestFromArchiveAsync_WhenManifestEntryIsMissing_ShouldReturnPackageInvalidError()
    {
        // Arrange
        string archivePath = Path.Combine(_testRootPath, "no-manifest.zip");
        File.WriteAllBytes(archivePath, _themePackFixture.CreateArchive(new ThemeArchiveEntry("templates/default.html", DEFAULT_TEMPLATE)));

        // Act
        Result<ThemeManifestDto> result = await _sut.ReadManifestFromArchiveAsync(archivePath, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("missing theme.json", result.FirstError.Description);
    }

    [Fact]
    public async Task ReadManifestFromArchiveAsync_WhenManifestIsNotValidJson_ShouldReturnInvalidJsonError()
    {
        // Arrange
        string archivePath = Path.Combine(_testRootPath, "invalid-json.zip");
        File.WriteAllBytes(archivePath, _themePackFixture.CreateArchive(new ThemeArchiveEntry("theme.json", "not json")));

        // Act
        Result<ThemeManifestDto> result = await _sut.ReadManifestFromArchiveAsync(archivePath, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Manifest.InvalidJson", result.FirstError.Code);
    }

    [Fact]
    public async Task ReadManifestFromArchiveAsync_WhenArchiveIsNotAZip_ShouldReturnThemeArchiveNotReadable()
    {
        // Arrange
        string archivePath = Path.Combine(_testRootPath, "not-a-zip.zip");
        File.WriteAllBytes(archivePath, Encoding.UTF8.GetBytes("plain text file"));

        // Act
        Result<ThemeManifestDto> result = await _sut.ReadManifestFromArchiveAsync(archivePath, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeArchiveNotReadable", result.FirstError.Description);
    }

    [Fact]
    public async Task ReadManifestFromArchiveAsync_WhenArchiveFileDoesNotExist_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        string archivePath = Path.Combine(_testRootPath, "does-not-exist.zip");

        // Act
        Result<ThemeManifestDto> result = await _sut.ReadManifestFromArchiveAsync(archivePath, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task DeleteAsync_WhenThemeIsInstalled_ShouldRemoveThemePack()
    {
        // Arrange
        await InstallValidThemeAsync(_themePackFixture.Create());

        // Act
        Result<Success> result = await _sut.DeleteAsync("test-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(_sut.HasThemePack("test-theme"));
        Assert.False(Directory.Exists(Path.Combine(_options.StoragePath, "test-theme")));
    }

    [Fact]
    public async Task DeleteAsync_WhenThemeIsNotInstalled_ShouldReturnSuccess()
    {
        // Arrange
        // nothing is installed

        // Act
        Result<Success> result = await _sut.DeleteAsync("test-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoadManifestAsync_WhenThemeIsNotInstalled_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        // nothing is installed

        // Act
        Result<ThemeManifestDto> result = await _sut.LoadManifestAsync("test-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task LoadManifestAsync_WhenThemeIsInstalled_ShouldReturnManifest()
    {
        // Arrange
        await InstallValidThemeAsync(_themePackFixture.Create(themeId: "loaded-theme"));

        // Act
        Result<ThemeManifestDto> result = await _sut.LoadManifestAsync("loaded-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("loaded-theme", result.Value.Id);
        Assert.Equal("Test Theme", result.Value.Name);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenPageKeyHasExplicitManifestTemplate_ShouldReturnExplicitTemplate()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "home", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HOME_TEMPLATE, result.Value);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenPageKeyHasNoExplicitTemplate_ShouldUseMirroredTemplate()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "library", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LIBRARY_TEMPLATE, result.Value);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenMirroredTemplateIsMissing_ShouldWalkUpToParentScope()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "library/book", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LIBRARY_TEMPLATE, result.Value);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenNoTemplateResolves_ShouldFallBackToDefaultTemplate()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "unknown-page", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DEFAULT_TEMPLATE, result.Value);
    }

    [Theory]
    [InlineData("../secret")] // parent scope traversal attempt
    [InlineData("/etc/passwd")] // rooted path attempt
    [InlineData("templates//nested")] // empty segment attempt
    public async Task GetTemplateAsync_WhenPageKeyAttemptsToEscapeTheThemePack_ShouldFallBackToDefaultTemplate(string pageKey)
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", pageKey, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DEFAULT_TEMPLATE, result.Value);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenThemeIsNotInstalled_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        // nothing is installed

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "home", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenManifestTemplateFileWasRemoved_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);
        File.Delete(Path.Combine(_options.StoragePath, "test-theme", "templates", "default.html"));

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "unknown-page", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains("missing file", result.FirstError.Description);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenMirroredTemplateFileWasRemoved_ShouldFallBackToDefaultTemplate()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);
        File.Delete(Path.Combine(_options.StoragePath, "test-theme", "templates", "library.html"));

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "library/book", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DEFAULT_TEMPLATE, result.Value);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenScriptsAreDisabled_ShouldStripScriptElements()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(defaultTemplateContent: "<script>alert('xss')</script><p>safe content</p>");
        await InstallValidThemeAsync(archive);

        // Act
        Result<string> result = await _sut.GetTemplateAsync("test-theme", "unknown-page", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("<p>safe content</p>", result.Value);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenScriptsAreEnabled_ShouldPreserveScriptElements()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.AllowThemeScripts = true;
        ThemeService sut = CreateSut(options);
        const string TEMPLATE_WITH_SCRIPT = "<script>alert('xss')</script><p>safe content</p>";
        byte[] archive = _themePackFixture.Create(defaultTemplateContent: TEMPLATE_WITH_SCRIPT);
        await InstallValidThemeAsync(archive, sut);

        // Act
        Result<string> result = await sut.GetTemplateAsync("test-theme", "unknown-page", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TEMPLATE_WITH_SCRIPT, result.Value);
    }

    [Fact]
    public async Task GetAssetAsync_WhenAssetExists_ShouldReturnFileBytesAndContentType()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(additionalFiles: new Dictionary<string, string>
        {
            ["assets/style.css"] = "body { color: red; }"
        });
        await InstallValidThemeAsync(archive);

        // Act
        Result<ThemeAssetDto> result = await _sut.GetAssetAsync("test-theme", "assets/style.css", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("text/css", result.Value.ContentType);
        Assert.Equal("body { color: red; }", Encoding.UTF8.GetString(result.Value.Bytes));
    }

    [Fact]
    public async Task GetAssetAsync_WhenAssetPathIsOutsideAssetsDirectory_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = CreateThemeWithTemplates();
        await InstallValidThemeAsync(archive);

        // Act
        Result<ThemeAssetDto> result = await _sut.GetAssetAsync("test-theme", "templates/default.html", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
    }

    [Fact]
    public async Task GetAssetAsync_WhenAssetPathAttemptsTraversal_ShouldReturnPackageInvalidError()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create();
        await InstallValidThemeAsync(archive);

        // Act
        Result<ThemeAssetDto> result = await _sut.GetAssetAsync("test-theme", "../secret.txt", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
    }

    [Fact]
    public async Task GetAssetAsync_WhenAssetFileIsMissing_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create();
        await InstallValidThemeAsync(archive);

        // Act
        Result<ThemeAssetDto> result = await _sut.GetAssetAsync("test-theme", "assets/missing.png", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task GetAssetAsync_WhenScriptsAreDisabledAndAssetIsJavaScript_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create(additionalFiles: new Dictionary<string, string>
        {
            ["assets/app.js"] = "console.log('hello');"
        });
        await InstallValidThemeAsync(archive);

        // Act
        Result<ThemeAssetDto> result = await _sut.GetAssetAsync("test-theme", "assets/app.js", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task GetAssetAsync_WhenScriptsAreEnabledAndAssetIsJavaScript_ShouldReturnJavaScriptContentType()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(
            storagePath: Path.Combine(_testRootPath, "themes"),
            bundledThemesPath: Path.Combine(_testRootPath, "bundled"),
            defaultThemeId: "lumina-default",
            maxArchiveBytes: 8 * 1024 * 1024,
            maxExpandedBytes: 24 * 1024 * 1024,
            maxSingleFileBytes: 6 * 1024 * 1024,
            maxEntries: 250,
            allowThemeScripts: false);
        options.AllowThemeScripts = true;
        ThemeService sut = CreateSut(options);
        byte[] archive = _themePackFixture.Create(additionalFiles: new Dictionary<string, string>
        {
            ["assets/app.js"] = "console.log('hello');"
        });
        await InstallValidThemeAsync(archive, sut);

        // Act
        Result<ThemeAssetDto> result = await sut.GetAssetAsync("test-theme", "assets/app.js", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("application/javascript", result.Value.ContentType);
    }

    [Fact]
    public async Task BuildArchiveAsync_WhenThemeIsInstalled_ShouldReturnDownloadableZipArchive()
    {
        // Arrange
        byte[] archive = _themePackFixture.Create();
        await InstallValidThemeAsync(archive);

        // Act
        Result<ThemeArchiveDto> result = await _sut.BuildArchiveAsync("test-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-theme.zip", result.Value.FileName);
        Assert.Equal("application/zip", result.Value.ContentType);
        using MemoryStream stream = new(result.Value.Bytes);
        using ZipArchive rebuiltArchive = new(stream, ZipArchiveMode.Read);
        Assert.Contains(rebuiltArchive.Entries, entry => entry.FullName == "theme.json");
        Assert.Contains(rebuiltArchive.Entries, entry => entry.FullName == "templates/default.html");
    }

    [Fact]
    public async Task BuildArchiveAsync_WhenThemeIsNotInstalled_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        // nothing is installed

        // Act
        Result<ThemeArchiveDto> result = await _sut.BuildArchiveAsync("test-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task RestoreBundledThemeAsync_WhenBundledArchiveMatchesThemeId_ShouldInstallTheme()
    {
        // Arrange
        string bundledPath = Path.Combine(_testRootPath, "bundled");
        Directory.CreateDirectory(bundledPath);
        File.WriteAllBytes(Path.Combine(bundledPath, "bundled-theme.zip"), _themePackFixture.Create(themeId: "bundled-theme"));

        // Act
        Result<Success> result = await _sut.RestoreBundledThemeAsync("bundled-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(_sut.HasThemePack("bundled-theme"));
    }

    [Fact]
    public async Task RestoreBundledThemeAsync_WhenNoBundledArchiveMatches_ShouldReturnThemeFilesUnreadable()
    {
        // Arrange
        string bundledPath = Path.Combine(_testRootPath, "bundled");
        Directory.CreateDirectory(bundledPath);
        File.WriteAllBytes(Path.Combine(bundledPath, "other-theme.zip"), _themePackFixture.Create(themeId: "other-theme"));

        // Act
        Result<Success> result = await _sut.RestoreBundledThemeAsync("bundled-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
    }

    [Fact]
    public async Task RestoreBundledThemeAsync_WhenBundledArchiveManifestIsInvalid_ShouldSkipArchiveAndReturnThemeFilesUnreadable()
    {
        // Arrange
        string bundledPath = Path.Combine(_testRootPath, "bundled");
        Directory.CreateDirectory(bundledPath);
        File.WriteAllBytes(Path.Combine(bundledPath, "broken.zip"), _themePackFixture.CreateArchive(new ThemeArchiveEntry("templates/default.html", DEFAULT_TEMPLATE)));

        // Act
        Result<Success> result = await _sut.RestoreBundledThemeAsync("bundled-theme", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ThemeFilesUnreadable", result.FirstError.Description);
        Assert.False(_sut.HasThemePack("bundled-theme"));
    }

    /// <summary>
    /// Creates a valid theme pack that declares the default, home and library templates and their files.
    /// </summary>
    /// <returns>The created theme pack ZIP archive.</returns>
    private byte[] CreateThemeWithTemplates()
    {
        Dictionary<string, string> templates = new(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = "templates/default.html",
            ["home"] = "templates/home.html"
        };

        return _themePackFixture.Create(
            templates: templates,
            additionalFiles: new Dictionary<string, string>
            {
                ["templates/library.html"] = LIBRARY_TEMPLATE
            });
    }

    /// <summary>
    /// Installs the provided theme archive using the provided service and asserts that the install succeeds.
    /// </summary>
    /// <param name="archive">The theme pack ZIP archive to install.</param>
    /// <param name="service">The service to install with, defaults to the shared system under test.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    private async Task InstallValidThemeAsync(byte[] archive, ThemeService? service = null)
    {
        ThemeService themeService = service ?? _sut;
        using MemoryStream stream = new(archive);
        Result<ThemeManifestDto> result = await themeService.InstallAsync(stream, CancellationToken.None);
        Assert.True(result.IsSuccess, result.Match(
            success => string.Empty,
            errors => $"Expected a successful install but got: {errors.First().Code}: {errors.First().Description}"));
    }

    /// <summary>
    /// Asserts that the result is a package invalid failure whose description contains the provided fragment.
    /// </summary>
    /// <param name="result">The result to assert on.</param>
    /// <param name="expectedDescriptionFragment">The fragment the error description must contain.</param>
    private static void AssertPackageInvalid(Result<ThemeManifestDto> result, string expectedDescriptionFragment)
    {
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Package.Invalid", result.FirstError.Code);
        Assert.Contains(expectedDescriptionFragment, result.FirstError.Description);
    }

    /// <summary>
    /// Creates a <see cref="ThemeService"/> instance using the provided options.
    /// </summary>
    /// <param name="options">The theme engine options to configure the service with.</param>
    /// <returns>The created service.</returns>
    private static ThemeService CreateSut(ThemeEngineOptionsDto options)
    {
        ILogger<ThemeService> logger = Substitute.For<ILogger<ThemeService>>();
        return new ThemeService(Options.Create(options), logger);
    }
}
