#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Manages the installed theme packages: installation, selection, rendering documents and archives.
/// </summary>
public sealed class ThemeService
{
    private const string MANIFEST_FILE_NAME = "theme.json";
    private const string INSTALLATION_FILE_NAME = ".theme-install.json";
    private const long MAX_MANIFEST_BYTES = 64 * 1024;

    private static readonly Regex s_themeIdPattern = new(
        "^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex s_templateKeyPattern = new(
        "^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex s_versionPattern = new(
        "^[0-9]+\\.[0-9]+\\.[0-9]+(?:[-+][A-Za-z0-9.-]+)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex s_pathSegmentPattern = new(
        "^[A-Za-z0-9][A-Za-z0-9._ -]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> s_assetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".ico",
        ".woff", ".woff2", ".ttf"
    };

    private static readonly JsonSerializerOptions s_manifestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 16
    };

    private static readonly JsonSerializerOptions s_metadataJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ThemeEngineOptionsDto _options;
    private readonly ThemeSettingsStore _settingsStore;
    private readonly ThemeTemplateEngine _templateEngine;
    private readonly ILogger<ThemeService> _logger;
    private readonly string _contentRoot;
    private readonly string _storageRoot;
    private readonly string _stagingRoot;
    private readonly ConcurrentDictionary<string, InstalledThemeDto> _themes = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _mutationGate = new(1, 1);
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="environment">The web host environment, used to resolve the content root.</param>
    /// <param name="options">The theme engine configuration options.</param>
    /// <param name="settingsStore">The store that persists the current theme selection.</param>
    /// <param name="templateEngine">The template engine used to validate installed templates.</param>
    /// <param name="logger">The logger for this service.</param>
    public ThemeService(
        IWebHostEnvironment environment,
        IOptions<ThemeEngineOptionsDto> options,
        ThemeSettingsStore settingsStore,
        ThemeTemplateEngine templateEngine,
        ILogger<ThemeService> logger)
    {
        _contentRoot = environment.ContentRootPath;
        _options = options.Value;
        _settingsStore = settingsStore;
        _templateEngine = templateEngine;
        _logger = logger;
        _storageRoot = ResolvePath(_contentRoot, _options.StoragePath);
        _stagingRoot = Path.Combine(_storageRoot, ".staging");
    }

    /// <summary>
    /// Gets a value indicating whether theme templates may load script files.
    /// </summary>
    public bool AllowThemeScripts => _options.AllowThemeScripts;

    /// <summary>
    /// Gets the maximum allowed size of a theme archive, in bytes.
    /// </summary>
    public long MaxArchiveBytes => _options.MaxArchiveBytes;

    /// <summary>
    /// Loads the bundled and previously installed themes into memory and selects a valid current theme.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _mutationGate.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
                return Result.Success;

            Directory.CreateDirectory(_storageRoot);
            Directory.CreateDirectory(_stagingRoot);
            CleanStagingDirectory();

            string sampleThemeDirectory = Path.Combine(_contentRoot, "SampleThemes");
            if (Directory.Exists(sampleThemeDirectory))
            {
                foreach (string? archivePath in Directory.EnumerateFiles(sampleThemeDirectory, "*.zip").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    await using FileStream archive = File.OpenRead(archivePath);
                    Result<InstalledThemeDto> installResult = await InstallCoreAsync(archive, skipIfInstalled: true, source: ThemeInstallSource.Bundled, cancellationToken);
                    if (installResult.IsFailure)
                        return Result<Success>.Failure(installResult.Errors);
                }
            }

            foreach (string themeDirectory in Directory.EnumerateDirectories(_storageRoot))
            {
                if (Path.GetFileName(themeDirectory).StartsWith('.'))
                    continue;

                Result<InstalledThemeDto> loadResult = await LoadInstalledThemeAsync(themeDirectory, cancellationToken);
                if (loadResult.IsFailure)
                {
                    _logger.LogWarning("Skipping invalid installed theme directory {ThemeDirectory}. {Error}", themeDirectory, loadResult.FirstError.Description);
                    continue;
                }

                _themes[loadResult.Value.Manifest.Id] = loadResult.Value;
            }

            if (_themes.IsEmpty)
                return Error.Failure(code: "Theme.NoInstalledThemes", description: "No valid themes are installed. Ensure the SampleThemes archives are present.");

            string? currentThemeId = await _settingsStore.GetCurrentThemeIdAsync(cancellationToken);
            if (currentThemeId is null || !_themes.ContainsKey(currentThemeId))
                await EnsureValidCurrentThemeAsync(cancellationToken);
            _initialized = true;
            return Result.Success;
        }
        finally
        {
            _mutationGate.Release();
        }
    }

    /// <summary>
    /// Gets the display metadata of all installed themes.
    /// </summary>
    /// <returns>The list of installed theme metadata.</returns>
    public IReadOnlyList<ThemeInfoDto> GetThemes()
    {
        return [.. _themes.Values
            .Select(theme => theme.Info)
            .OrderByDescending(theme => theme.IsBundled)
            .ThenBy(theme => theme.Name, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Determines whether a theme with the specified identifier is installed.
    /// </summary>
    /// <param name="themeId">The theme identifier to look up.</param>
    /// <returns>true when the theme is installed; otherwise, false.</returns>
    public bool IsInstalled(string themeId)
    {
        return !string.IsNullOrWhiteSpace(themeId) && _themes.ContainsKey(themeId);
    }

    /// <summary>
    /// Gets the identifier of the currently selected theme, resolving a valid selection when needed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The identifier of the current theme.</returns>
    public async Task<string> GetCurrentThemeIdAsync(CancellationToken cancellationToken = default)
    {
        string? current = await _settingsStore.GetCurrentThemeIdAsync(cancellationToken);
        if (current is not null && _themes.ContainsKey(current))
            return current;

        return await EnsureValidCurrentThemeAsync(cancellationToken);
    }

    /// <summary>
    /// Selects the theme with the specified identifier as the current theme.
    /// </summary>
    /// <param name="themeId">The identifier of the theme to select.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> SetCurrentThemeAsync(string themeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(themeId) || !_themes.ContainsKey(themeId))
            return ThemeNotFound(themeId);

        await _settingsStore.SetCurrentThemeIdAsync(themeId, cancellationToken);
        return Result.Success;
    }

    /// <summary>
    /// Installs a theme from the provided archive.
    /// </summary>
    /// <param name="archive">The ZIP archive stream of the theme package.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the installed theme metadata, or an error.</returns>
    public async Task<Result<ThemeInfoDto>> InstallAsync(Stream archive, CancellationToken cancellationToken = default)
    {
        await _mutationGate.WaitAsync(cancellationToken);
        try
        {
            Result<InstalledThemeDto> installedResult = await InstallCoreAsync(archive, skipIfInstalled: false, source: ThemeInstallSource.Uploaded, cancellationToken);
            if (installedResult.IsFailure)
                return Result<ThemeInfoDto>.Failure(installedResult.Errors);

            return installedResult.Value.Info;
        }
        finally
        {
            _mutationGate.Release();
        }
    }

    /// <summary>
    /// Gets the theme and raw template source selected for a page key.
    /// </summary>
    /// <param name="pageKey">The page key that selects the template to render.</param>
    /// <param name="requestedThemeId">The optional theme to render with, falling back to the current theme when null.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the render document, or an error.</returns>
    public async Task<Result<ThemeRenderDocumentDto>> GetRenderDocumentAsync(string pageKey, string? requestedThemeId, CancellationToken cancellationToken = default)
    {
        InstalledThemeDto installedTheme;
        if (!string.IsNullOrWhiteSpace(requestedThemeId))
        {
            if (!_themes.TryGetValue(requestedThemeId, out InstalledThemeDto? requestedTheme))
                return ThemeNotFound(requestedThemeId);

            installedTheme = requestedTheme;
        }
        else
        {
            string currentThemeId = await GetCurrentThemeIdAsync(cancellationToken);
            installedTheme = _themes[currentThemeId];
        }

        if (!installedTheme.Manifest.Templates.TryGetValue(pageKey, out string? templatePath))
            templatePath = installedTheme.Manifest.Templates["default"];

        Result<string> fullTemplatePathResult = ResolveContainedPath(installedTheme.RootPath, templatePath);
        if (fullTemplatePathResult.IsFailure)
            return Result<ThemeRenderDocumentDto>.Failure(fullTemplatePathResult.Errors);

        string template = await File.ReadAllTextAsync(fullTemplatePathResult.Value, cancellationToken);
        return new ThemeRenderDocumentDto(installedTheme.Info, template);
    }

    /// <summary>
    /// Resolves the absolute path of an allowed theme asset, when it exists.
    /// </summary>
    /// <param name="themeId">The theme identifier.</param>
    /// <param name="assetPath">The asset path relative to the theme package root.</param>
    /// <param name="fullPath">The resolved absolute path of the asset, empty when not resolved.</param>
    /// <returns>true when the asset was resolved; otherwise, false.</returns>
    public bool TryResolveAsset(string themeId, string assetPath, out string fullPath)
    {
        fullPath = string.Empty;
        if (!_themes.TryGetValue(themeId, out InstalledThemeDto? installedTheme))
            return false;

        Result<string> normalizedPathResult = NormalizeRelativePath(assetPath);
        if (normalizedPathResult.IsFailure)
            return false;

        string normalizedPath = normalizedPathResult.Value;
        if (!normalizedPath.StartsWith("assets/", StringComparison.Ordinal))
            return false;

        string extension = Path.GetExtension(normalizedPath);
        if (!IsAllowedAssetExtension(extension))
            return false;

        Result<string> candidateResult = ResolveContainedPath(installedTheme.RootPath, normalizedPath);
        if (candidateResult.IsFailure)
            return false;

        if (!File.Exists(candidateResult.Value))
            return false;

        fullPath = candidateResult.Value;
        return true;
    }

    /// <summary>
    /// Builds a downloadable ZIP archive of an installed theme.
    /// </summary>
    /// <param name="themeId">The identifier of the theme to archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the theme archive, or an error.</returns>
    public async Task<Result<ThemeArchiveDto>> BuildArchiveAsync(string themeId, CancellationToken cancellationToken = default)
    {
        if (!_themes.TryGetValue(themeId, out InstalledThemeDto? installedTheme))
            return ThemeNotFound(themeId);

        MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (string? filePath in Directory.EnumerateFiles(installedTheme.RootPath, "*", SearchOption.AllDirectories)
                     .Where(path => !string.Equals(Path.GetFileName(path), INSTALLATION_FILE_NAME, StringComparison.Ordinal))
                     .OrderBy(path => path, StringComparer.Ordinal))
            {
                string relativePath = Path.GetRelativePath(installedTheme.RootPath, filePath).Replace(Path.DirectorySeparatorChar, '/');
                ZipArchiveEntry entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using FileStream input = File.OpenRead(filePath);
                await using Stream outputStream = entry.Open();
                await input.CopyToAsync(outputStream, cancellationToken);
            }
        }

        output.Position = 0;
        string safeVersion = installedTheme.Manifest.Version.Replace('+', '-');
        return new ThemeArchiveDto($"{installedTheme.Manifest.Id}-{safeVersion}.zip", output);
    }

    /// <summary>
    /// Installs a theme package from an archive stream into the storage root.
    /// </summary>
    /// <param name="sourceArchive">The ZIP archive stream of the theme package.</param>
    /// <param name="skipIfInstalled">Whether an already installed theme with the same id is accepted without reinstalling.</param>
    /// <param name="source">The source the theme is installed from.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the installed theme, or an error.</returns>
    private async Task<Result<InstalledThemeDto>> InstallCoreAsync(Stream sourceArchive, bool skipIfInstalled, ThemeInstallSource source, CancellationToken cancellationToken)
    {
        string workId = Guid.NewGuid().ToString("N");
        string temporaryArchivePath = Path.Combine(_stagingRoot, $"{workId}.zip");
        string extractionPath = Path.Combine(_stagingRoot, workId);

        try
        {
            Result<Success> copyResult = await CopyArchiveWithLimitAsync(sourceArchive, temporaryArchivePath, cancellationToken);
            if (copyResult.IsFailure)
                return Result<InstalledThemeDto>.Failure(copyResult.Errors);

            Directory.CreateDirectory(extractionPath);
            Result<Success> extractResult = await ExtractArchiveAsync(temporaryArchivePath, extractionPath, cancellationToken);
            if (extractResult.IsFailure)
                return Result<InstalledThemeDto>.Failure(extractResult.Errors);

            Result<InstalledThemeDto> stagedResult = await LoadInstalledThemeAsync(extractionPath, cancellationToken, sourceOverride: source);
            if (stagedResult.IsFailure)
                return Result<InstalledThemeDto>.Failure(stagedResult.Errors);

            InstalledThemeDto stagedTheme = stagedResult.Value;
            string destination = Path.Combine(_storageRoot, stagedTheme.Manifest.Id);

            if (Directory.Exists(destination))
            {
                if (!skipIfInstalled)
                    return Error.Conflict(code: "Theme.AlreadyInstalled", description: $"A theme with id '{stagedTheme.Manifest.Id}' is already installed.");

                Result<InstalledThemeDto> existingResult = await LoadInstalledThemeAsync(destination, cancellationToken);
                if (existingResult.IsFailure)
                    return Result<InstalledThemeDto>.Failure(existingResult.Errors);

                _themes[existingResult.Value.Manifest.Id] = existingResult.Value;
                return existingResult.Value;
            }

            ThemeInstallationMetadataDto metadata = new()
            {
                Source = source,
                InstalledAtUtc = DateTimeOffset.UtcNow
            };
            string metadataJson = JsonSerializer.Serialize(metadata, s_metadataJsonOptions);
            await File.WriteAllTextAsync(Path.Combine(extractionPath, INSTALLATION_FILE_NAME), metadataJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);

            Directory.Move(extractionPath, destination);
            Result<InstalledThemeDto> installedResult = await LoadInstalledThemeAsync(destination, cancellationToken);
            if (installedResult.IsFailure)
                return Result<InstalledThemeDto>.Failure(installedResult.Errors);

            _themes[installedResult.Value.Manifest.Id] = installedResult.Value;
            return installedResult.Value;
        }
        catch (InvalidDataException exception)
        {
            _logger.LogWarning(exception, "An uploaded theme archive could not be read.");
            return Error.Failure(code: "Theme.Archive.ReadFailed", description: "The uploaded file is not a readable ZIP archive.");
        }
        finally
        {
            if (File.Exists(temporaryArchivePath))
                File.Delete(temporaryArchivePath);

            if (Directory.Exists(extractionPath))
                Directory.Delete(extractionPath, recursive: true);
        }
    }

    /// <summary>
    /// Copies the source archive to the staging directory while enforcing the maximum archive size.
    /// </summary>
    /// <param name="source">The archive stream to copy.</param>
    /// <param name="destinationPath">The temporary file to copy the archive into.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> CopyArchiveWithLimitAsync(Stream source, string destinationPath, CancellationToken cancellationToken)
    {
        await using FileStream destination = new(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024, useAsync: true);

        byte[] buffer = new byte[64 * 1024];
        long totalBytes = 0;
        while (true)
        {
            int read = await source.ReadAsync(buffer.AsMemory(), cancellationToken);
            if (read == 0)
                break;

            totalBytes += read;
            if (totalBytes > _options.MaxArchiveBytes)
                return Error.Validation(code: "Theme.Archive.TooLarge", description: $"The theme archive exceeds the {_options.MaxArchiveBytes / 1024 / 1024} MB limit.");

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (totalBytes == 0)
            return Error.Validation(code: "Theme.Archive.Empty", description: "The uploaded theme archive is empty.");

        return Result.Success;
    }

    /// <summary>
    /// Extracts a staged theme archive into the staging extraction directory after validating its entries.
    /// </summary>
    /// <param name="archivePath">The path of the staged ZIP archive.</param>
    /// <param name="extractionPath">The directory the archive is extracted into.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ExtractArchiveAsync(string archivePath, string extractionPath, CancellationToken cancellationToken)
    {
        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        if (archive.Entries.Count == 0)
            return Error.Validation(code: "Theme.Archive.Empty", description: "The theme archive contains no files.");

        if (archive.Entries.Count > _options.MaxEntries)
            return Error.Validation(code: "Theme.Archive.TooManyEntries", description: $"The theme archive contains more than {_options.MaxEntries} entries.");

        HashSet<string> seenPaths = new(StringComparer.OrdinalIgnoreCase);
        long expandedBytes = 0;

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int unixFileType = (entry.ExternalAttributes >> 16) & 0xF000;
            if (unixFileType == 0xA000)
                return PackageInvalid("Symbolic links are not allowed in theme archives.");

            Result<string> normalizedPathResult = NormalizeRelativePath(entry.FullName, allowTrailingSlash: true);
            if (normalizedPathResult.IsFailure)
                return Result<Success>.Failure(normalizedPathResult.Errors);

            string normalizedPath = normalizedPathResult.Value;
            if (!seenPaths.Add(normalizedPath))
                return PackageInvalid($"The archive contains a duplicate path: {normalizedPath}");

            bool isDirectory = entry.FullName.EndsWith('/') || string.IsNullOrEmpty(entry.Name);
            if (isDirectory)
            {
                Result<Success> directoryValidationResult = ValidatePackageDirectory(normalizedPath);
                if (directoryValidationResult.IsFailure)
                    return Result<Success>.Failure(directoryValidationResult.Errors);

                Result<string> directoryPathResult = ResolveContainedPath(extractionPath, normalizedPath);
                if (directoryPathResult.IsFailure)
                    return Result<Success>.Failure(directoryPathResult.Errors);

                Directory.CreateDirectory(directoryPathResult.Value);
                continue;
            }

            Result<Success> fileValidationResult = ValidatePackageFile(normalizedPath);
            if (fileValidationResult.IsFailure)
                return Result<Success>.Failure(fileValidationResult.Errors);

            if (entry.Length > _options.MaxSingleFileBytes)
                return Error.Validation(code: "Theme.File.TooLarge", description: $"Theme file '{normalizedPath}' is too large.");

            expandedBytes += entry.Length;
            if (expandedBytes > _options.MaxExpandedBytes)
                return Error.Validation(code: "Theme.Archive.ExpandedTooLarge", description: "The expanded theme archive is too large.");

            Result<string> outputPathResult = ResolveContainedPath(extractionPath, normalizedPath);
            if (outputPathResult.IsFailure)
                return Result<Success>.Failure(outputPathResult.Errors);

            string outputPath = outputPathResult.Value;
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            await using Stream input = entry.Open();
            await using FileStream output = new(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024, useAsync: true);

            byte[] buffer = new byte[64 * 1024];
            long entryBytes = 0;
            while (true)
            {
                int read = await input.ReadAsync(buffer.AsMemory(), cancellationToken);
                if (read == 0)
                    break;

                entryBytes += read;
                if (entryBytes > _options.MaxSingleFileBytes)
                    return Error.Validation(code: "Theme.File.TooLarge", description: $"Theme file '{normalizedPath}' is too large.");

                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }

        if (!File.Exists(Path.Combine(extractionPath, MANIFEST_FILE_NAME)))
            return PackageInvalid("The archive must contain theme.json at its root.");

        return Result.Success;
    }

    /// <summary>
    /// Loads and validates the manifest, templates and metadata of an installed theme directory.
    /// </summary>
    /// <param name="themeRoot">The absolute path of the installed theme directory.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <param name="sourceOverride">The install source to report, overriding the persisted metadata when provided.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the installed theme, or an error.</returns>
    private async Task<Result<InstalledThemeDto>> LoadInstalledThemeAsync(string themeRoot, CancellationToken cancellationToken, ThemeInstallSource? sourceOverride = null)
    {
        string manifestPath = Path.Combine(themeRoot, MANIFEST_FILE_NAME);
        if (!File.Exists(manifestPath))
            return PackageInvalid("The theme is missing theme.json.");

        if (new FileInfo(manifestPath).Length > MAX_MANIFEST_BYTES)
            return Error.Validation(code: "Theme.Manifest.TooLarge", description: "theme.json exceeds the 64 KB limit.");

        string json;
        ThemeManifestDto? manifest;
        try
        {
            json = await File.ReadAllTextAsync(manifestPath, cancellationToken);
            manifest = JsonSerializer.Deserialize<ThemeManifestDto>(json, s_manifestJsonOptions);
        }
        catch (JsonException)
        {
            return Error.Validation(code: "Theme.Manifest.InvalidJson", description: "theme.json is not valid JSON.");
        }
        catch (IOException)
        {
            return Error.Failure(code: "Theme.Files.Unreadable", description: "The theme files could not be read.");
        }

        if (manifest is null)
            return Error.Validation(code: "Theme.Manifest.InvalidJson", description: "theme.json contains no manifest object.");

        Result<Success> manifestValidationResult = ValidateManifest(themeRoot, manifest);
        if (manifestValidationResult.IsFailure)
            return Result<InstalledThemeDto>.Failure(manifestValidationResult.Errors);

        foreach (string templatePath in manifest.Templates.Values.Distinct(StringComparer.Ordinal))
        {
            Result<string> fullTemplatePathResult = ResolveContainedPath(themeRoot, templatePath);
            if (fullTemplatePathResult.IsFailure)
                return Result<InstalledThemeDto>.Failure(fullTemplatePathResult.Errors);

            try
            {
                string template = await File.ReadAllTextAsync(fullTemplatePathResult.Value, cancellationToken);
                Result<Success> templateValidationResult = _templateEngine.Validate(template);
                if (templateValidationResult.IsFailure)
                    return Result<InstalledThemeDto>.Failure(templateValidationResult.Errors);
            }
            catch (IOException)
            {
                return Error.Failure(code: "Theme.Files.Unreadable", description: "The theme files could not be read.");
            }
        }

        ThemeInstallSource source = sourceOverride ?? await ReadInstallSourceAsync(themeRoot, cancellationToken);
        string previewUrl = string.IsNullOrWhiteSpace(manifest.Preview) ? "/admin/theme-placeholder.svg" : $"/theme-assets/{manifest.Id}/{manifest.Preview}";
        ThemeInfoDto info = new(manifest.Id, manifest.Name, manifest.Description, manifest.Author, manifest.Version, previewUrl, source == ThemeInstallSource.Bundled);

        return new InstalledThemeDto(manifest, info, Path.GetFullPath(themeRoot));
    }

    /// <summary>
    /// Validates the manifest fields and template mappings, normalizing the referenced paths.
    /// </summary>
    /// <param name="themeRoot">The absolute path of the theme package root.</param>
    /// <param name="manifest">The manifest to validate.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private Result<Success> ValidateManifest(string themeRoot, ThemeManifestDto manifest)
    {
        if (manifest.SchemaVersion != 1)
            return PackageInvalid("Only theme schemaVersion 1 is supported.");

        if (manifest.Id is null || !s_themeIdPattern.IsMatch(manifest.Id) || manifest.Id.Length > 64)
            return PackageInvalid("Theme id must be a lowercase kebab-case value up to 64 characters.");

        Result<Success> nameResult = ValidateText(manifest.Name, "name", 80);
        if (nameResult.IsFailure)
            return nameResult;

        Result<Success> descriptionResult = ValidateText(manifest.Description, "description", 300);
        if (descriptionResult.IsFailure)
            return descriptionResult;

        Result<Success> authorResult = ValidateText(manifest.Author, "author", 80);
        if (authorResult.IsFailure)
            return authorResult;

        Result<Success> versionResult = ValidateText(manifest.Version, "version", 40);
        if (versionResult.IsFailure)
            return versionResult;

        if (!s_versionPattern.IsMatch(manifest.Version))
            return PackageInvalid("Theme version must use semantic version form, for example 1.0.0.");

        if (manifest.Templates is null || manifest.Templates.Count == 0 || manifest.Templates.Count > 32)
            return PackageInvalid("A theme must declare between 1 and 32 templates.");

        Dictionary<string, string> normalizedTemplates = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, string path) in manifest.Templates)
        {
            if (!s_templateKeyPattern.IsMatch(key) || key.Length > 40)
                return PackageInvalid($"Template key '{key}' is invalid.");

            Result<string> normalizedPathResult = NormalizeRelativePath(path);
            if (normalizedPathResult.IsFailure)
                return Result<Success>.Failure(normalizedPathResult.Errors);

            string normalizedPath = normalizedPathResult.Value;
            if (!normalizedPath.StartsWith("templates/", StringComparison.Ordinal) || !string.Equals(Path.GetExtension(normalizedPath), ".html", StringComparison.OrdinalIgnoreCase))
                return PackageInvalid($"Template '{key}' must point to an HTML file under templates/.");

            Result<string> fullTemplatePathResult = ResolveContainedPath(themeRoot, normalizedPath);
            if (fullTemplatePathResult.IsFailure)
                return Result<Success>.Failure(fullTemplatePathResult.Errors);

            if (!File.Exists(fullTemplatePathResult.Value))
                return PackageInvalid($"Template '{key}' points to missing file '{normalizedPath}'.");

            normalizedTemplates[key] = normalizedPath;
        }

        if (!normalizedTemplates.ContainsKey("default"))
            return PackageInvalid("The manifest must define a 'default' template fallback.");

        manifest.Templates = normalizedTemplates;

        if (!string.IsNullOrWhiteSpace(manifest.Preview))
        {
            Result<string> previewPathResult = NormalizeRelativePath(manifest.Preview);
            if (previewPathResult.IsFailure)
                return Result<Success>.Failure(previewPathResult.Errors);

            string previewPath = previewPathResult.Value;
            Result<string> fullPreviewPathResult = ResolveContainedPath(themeRoot, previewPath);
            if (fullPreviewPathResult.IsFailure)
                return Result<Success>.Failure(fullPreviewPathResult.Errors);

            if (!previewPath.StartsWith("assets/", StringComparison.Ordinal) || !IsAllowedAssetExtension(Path.GetExtension(previewPath)) || !File.Exists(fullPreviewPathResult.Value))
                return PackageInvalid("The preview must reference an existing asset file.");

            manifest.Preview = previewPath;
        }

        return Result.Success;
    }

    /// <summary>
    /// Reads the persisted install source of a theme directory, defaulting to Uploaded when absent or invalid.
    /// </summary>
    /// <param name="themeRoot">The absolute path of the installed theme directory.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The install source of the theme.</returns>
    private static async Task<ThemeInstallSource> ReadInstallSourceAsync(string themeRoot, CancellationToken cancellationToken)
    {
        string metadataPath = Path.Combine(themeRoot, INSTALLATION_FILE_NAME);
        if (!File.Exists(metadataPath))
            return ThemeInstallSource.Uploaded;

        try
        {
            string json = await File.ReadAllTextAsync(metadataPath, cancellationToken);
            return JsonSerializer.Deserialize<ThemeInstallationMetadataDto>(json, s_metadataJsonOptions)?.Source ?? ThemeInstallSource.Uploaded;
        }
        catch (JsonException)
        {
            return ThemeInstallSource.Uploaded;
        }
    }

    /// <summary>
    /// Selects and persists a valid current theme, preferring the configured default theme.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The identifier of the selected theme.</returns>
    private async Task<string> EnsureValidCurrentThemeAsync(CancellationToken cancellationToken)
    {
        string preferred = _options.DefaultThemeId;
        string selected = _themes.ContainsKey(preferred) ? preferred : _themes.Values.OrderBy(theme => theme.Info.Name, StringComparer.OrdinalIgnoreCase).First().Manifest.Id;
        await _settingsStore.SetCurrentThemeIdAsync(selected, cancellationToken);
        return selected;
    }

    /// <summary>
    /// Validates that a package file resides in the manifest, template or asset locations.
    /// </summary>
    /// <param name="normalizedPath">The normalized package-relative path of the file.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private Result<Success> ValidatePackageFile(string normalizedPath)
    {
        if (string.Equals(normalizedPath, MANIFEST_FILE_NAME, StringComparison.Ordinal))
            return Result.Success;

        if (normalizedPath.StartsWith("templates/", StringComparison.Ordinal) && string.Equals(Path.GetExtension(normalizedPath), ".html", StringComparison.OrdinalIgnoreCase))
            return Result.Success;

        if (normalizedPath.StartsWith("assets/", StringComparison.Ordinal) && IsAllowedAssetExtension(Path.GetExtension(normalizedPath)))
            return Result.Success;

        return PackageInvalid($"Theme file '{normalizedPath}' is outside the allowed manifest, template, or asset locations.");
    }

    /// <summary>
    /// Validates that a package directory resides under the templates or assets locations.
    /// </summary>
    /// <param name="normalizedPath">The normalized package-relative path of the directory.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> ValidatePackageDirectory(string normalizedPath)
    {
        if (normalizedPath.Equals("templates", StringComparison.Ordinal) ||
            normalizedPath.StartsWith("templates/", StringComparison.Ordinal) ||
            normalizedPath.Equals("assets", StringComparison.Ordinal) ||
            normalizedPath.StartsWith("assets/", StringComparison.Ordinal))
            return Result.Success;

        return PackageInvalid($"Theme directory '{normalizedPath}' is not allowed.");
    }

    /// <summary>
    /// Determines whether the file extension is an allowed theme asset extension.
    /// </summary>
    /// <param name="extension">The file extension, including the leading dot.</param>
    /// <returns>true when the extension is allowed; otherwise, false.</returns>
    private bool IsAllowedAssetExtension(string extension)
    {
        return s_assetExtensions.Contains(extension) || (_options.AllowThemeScripts && string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normalizes a package-relative path, rejecting paths that escape the package or violate the path rules.
    /// </summary>
    /// <param name="path">The package-relative path to normalize.</param>
    /// <param name="allowTrailingSlash">Whether a trailing slash, used for directory entries, is permitted.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the normalized path, or an error.</returns>
    private static Result<string> NormalizeRelativePath(string path, bool allowTrailingSlash = false)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 240 || path.Contains('\\') || path.Contains('\0') || path.StartsWith('/') || path.Contains("//"))
            return PackageInvalid("The theme contains an invalid path.");

        string candidate = allowTrailingSlash ? path.TrimEnd('/') : path;
        if (string.IsNullOrWhiteSpace(candidate))
            return PackageInvalid("The theme contains an invalid path.");

        string[] segments = candidate.Split('/');
        if (segments.Length > 12)
            return PackageInvalid("A theme path is nested too deeply.");

        foreach (string segment in segments)
            if (segment is "." or ".." || segment.Length > 80 || segment.EndsWith(' ') || segment.EndsWith('.') || !s_pathSegmentPattern.IsMatch(segment))
                return PackageInvalid($"Theme path segment '{segment}' is invalid.");

        return string.Join('/', segments);
    }

    /// <summary>
    /// Resolves a package-relative path against a package root, rejecting paths that escape the root.
    /// </summary>
    /// <param name="root">The absolute path of the package root.</param>
    /// <param name="relativePath">The package-relative path to resolve.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the absolute path, or an error.</returns>
    private static Result<string> ResolveContainedPath(string root, string relativePath)
    {
        string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        string rootPath = Path.GetFullPath(root);
        string candidate = Path.GetFullPath(Path.Combine(rootPath, normalized));
        string rootWithSeparator = rootPath.EndsWith(Path.DirectorySeparatorChar) ? rootPath : rootPath + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (!candidate.StartsWith(rootWithSeparator, comparison))
            return PackageInvalid("A theme path escapes its package root.");

        return candidate;
    }

    /// <summary>
    /// Validates a manifest text field: required, within the maximum length and without control characters.
    /// </summary>
    /// <param name="value">The field value to validate.</param>
    /// <param name="fieldName">The name of the manifest field, used in error messages.</param>
    /// <param name="maxLength">The maximum allowed length of the field.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> ValidateText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength || value.Any(char.IsControl))
            return PackageInvalid($"Theme manifest field '{fieldName}' is required and must be at most {maxLength} characters.");

        return Result.Success;
    }

    /// <summary>
    /// Creates a validation error for an invalid theme package.
    /// </summary>
    /// <param name="description">The human-readable description of the package error.</param>
    /// <returns>The validation error describing the invalid theme package.</returns>
    private static Error PackageInvalid(string description)
    {
        return Error.Validation(code: "Theme.Package.Invalid", description: description);
    }

    /// <summary>
    /// Creates a not found error for a theme identifier that is not installed.
    /// </summary>
    /// <param name="themeId">The missing theme identifier.</param>
    /// <returns>The not found error describing the missing theme.</returns>
    private static Error ThemeNotFound(string themeId)
    {
        return Error.NotFound(code: "Theme.NotFound", description: $"The theme '{themeId}' is not installed.");
    }

    /// <summary>
    /// Removes stale files and directories left over in the staging directory.
    /// </summary>
    private void CleanStagingDirectory()
    {
        foreach (string path in Directory.EnumerateFileSystemEntries(_stagingRoot))
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
                else
                    File.Delete(path);
            }
            catch (IOException exception)
            {
                _logger.LogDebug(exception, "Could not remove stale theme staging path {StagingPath}.", path);
            }
        }
    }

    /// <summary>
    /// Resolves a configured path to an absolute path, rooted against the content root when relative.
    /// </summary>
    /// <param name="contentRoot">The content root of the application.</param>
    /// <param name="configuredPath">The path from the configuration.</param>
    /// <returns>The absolute path.</returns>
    private static string ResolvePath(string contentRoot, string configuredPath)
    {
        return Path.GetFullPath(Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(contentRoot, configuredPath));
    }
}
