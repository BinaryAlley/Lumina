#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
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

namespace Lumina.Infrastructure.Core.Themes;

/// <summary>
/// Stores and serves theme packs on the server, validating the pack envelope at install time and sanitizing served content.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private const string MANIFEST_FILE_NAME = "theme.json";
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

    // strips every script element from a template, both the inline content and the src attribute variants,
    // so that no script bundled in a theme is executed in any client when scripts are disabled
    private static readonly Regex s_scriptElementPattern = new(
        "<script\\b[^>]*>[\\s\\S]*?</script\\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions s_manifestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 16
    };

    private readonly ThemeEngineOptionsDto _options;
    private readonly ILogger<ThemeService> _logger;
    private readonly string _storageRoot;
    private readonly string _stagingRoot;
    private readonly string _bundledRoot;
    // serializes theme pack mutations, so concurrent installs and deletes cannot race on the same files; the gate is
    // shared by every instance that writes to the same storage root, because several hosts can run against the same
    // storage directory at once (for example the parallel in-memory hosts started by the test factories). A per-instance
    // gate is NOT enough: concurrent hosts raced on the shared staging cleanup and one host's directory move failed with
    // an access denied exception, which crashed that host and disposed its service provider. Keep the gate keyed on the
    // resolved storage root.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_storageGates = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _mutationGate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="options">The theme engine configuration options.</param>
    /// <param name="logger">The logger for this service.</param>
    public ThemeService(IOptions<ThemeEngineOptionsDto> options, ILogger<ThemeService> logger)
    {
        _options = options.Value;
        _logger = logger;
        string basePath = AppContext.BaseDirectory;
        _storageRoot = ResolvePath(basePath, _options.StoragePath);
        _stagingRoot = Path.Combine(_storageRoot, ".staging");
        _bundledRoot = ResolvePath(basePath, _options.BundledThemesPath);
        _mutationGate = s_storageGates.GetOrAdd(_storageRoot, static _ => new SemaphoreSlim(1, 1));
    }

    /// <summary>
    /// Gets a value indicating whether theme templates may contain script elements and theme assets may include script files.
    /// </summary>
    public bool AllowThemeScripts => _options.AllowThemeScripts;

    /// <summary>
    /// Gets the maximum allowed size of a theme archive, in bytes.
    /// </summary>
    public long MaxArchiveBytes => _options.MaxArchiveBytes;

    /// <summary>
    /// Gets the identifier of the theme selected when no valid current theme is available.
    /// </summary>
    public string DefaultThemeId => _options.DefaultThemeId;

    /// <summary>
    /// Gets the paths of the theme pack archives shipped with the application.
    /// </summary>
    /// <returns>The list of bundled theme archive paths.</returns>
    public IReadOnlyList<string> GetBundledThemeArchivePaths()
    {
        if (!Directory.Exists(_bundledRoot))
            return [];

        return [.. Directory.EnumerateFiles(_bundledRoot, "*.zip").OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Checks whether the stored pack files of a theme still exist.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <returns><see langword="true"/> when the theme pack files exist, <see langword="false"/> otherwise.</returns>
    public bool HasThemePack(string themeId)
    {
        string themePath = ResolveThemePath(themeId);
        return Directory.Exists(themePath) && File.Exists(Path.Combine(themePath, MANIFEST_FILE_NAME));
    }

    /// <summary>
    /// Installs a theme pack from the provided archive, replacing the files of an existing theme with the same manifest id.
    /// </summary>
    /// <param name="archive">The ZIP archive stream of the theme pack.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest of the installed theme, or an error.</returns>
    public async Task<Result<ThemeManifestDto>> InstallAsync(Stream archive, CancellationToken cancellationToken)
    {
        await _mutationGate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_storageRoot);
            Directory.CreateDirectory(_stagingRoot);
            CleanStagingDirectory();

            string workId = Guid.NewGuid().ToString("N");
            string temporaryArchivePath = Path.Combine(_stagingRoot, $"{workId}.zip");
            string extractionPath = Path.Combine(_stagingRoot, workId);

            try
            {
                Result<Success> copyResult = await CopyArchiveWithLimitAsync(archive, temporaryArchivePath, cancellationToken);
                if (copyResult.IsFailure)
                    return copyResult.Errors;

                Directory.CreateDirectory(extractionPath);
                Result<Success> extractResult = await ExtractArchiveAsync(temporaryArchivePath, extractionPath, cancellationToken);
                if (extractResult.IsFailure)
                    return extractResult.Errors;

                Result<ThemeManifestDto> manifestResult = LoadManifestFromDirectory(extractionPath);
                if (manifestResult.IsFailure)
                    return manifestResult.Errors;

                // install means replace: the files of an existing theme with the same manifest id are swapped with the new pack
                string destination = Path.Combine(_storageRoot, manifestResult.Value.Id);
                await SwapIntoDestinationAsync(extractionPath, destination, cancellationToken);
                return manifestResult.Value;
            }
            catch (InvalidDataException exception)
            {
                _logger.LogWarning(exception, "An uploaded theme archive could not be read.");
                return Errors.Themes.ThemeArchiveNotReadable;
            }
            finally
            {
                if (File.Exists(temporaryArchivePath))
                    File.Delete(temporaryArchivePath);

                if (Directory.Exists(extractionPath))
                    Directory.Delete(extractionPath, recursive: true);
            }
        }
        finally
        {
            _mutationGate.Release();
        }
    }

    /// <summary>
    /// Reads the manifest of a theme pack archive without installing it.
    /// </summary>
    /// <param name="archivePath">The path of the theme pack ZIP archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest, or an error.</returns>
    public async Task<Result<ThemeManifestDto>> ReadManifestFromArchiveAsync(string archivePath, CancellationToken cancellationToken)
    {
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(archivePath);
            ZipArchiveEntry? manifestEntry = archive.GetEntry(MANIFEST_FILE_NAME);
            if (manifestEntry is null)
                return PackageInvalid("The theme is missing theme.json.");

            await using Stream manifestStream = manifestEntry.Open();
            using StreamReader reader = new(manifestStream);
            string json = await reader.ReadToEndAsync(cancellationToken);
            return DeserializeManifest(json);
        }
        catch (InvalidDataException)
        {
            return Errors.Themes.ThemeArchiveNotReadable;
        }
        catch (IOException)
        {
            return Errors.Themes.ThemeFilesUnreadable;
        }
    }

    /// <summary>
    /// Deletes the stored files of a theme pack.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> DeleteAsync(string themeId, CancellationToken cancellationToken)
    {
        await _mutationGate.WaitAsync(cancellationToken);
        try
        {
            string themePath = ResolveThemePath(themeId);
            if (Directory.Exists(themePath))
                Directory.Delete(themePath, recursive: true);

            return Result.Success;
        }
        finally
        {
            _mutationGate.Release();
        }
    }

    /// <summary>
    /// Loads the manifest of an installed theme pack from its storage location.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest, or an error.</returns>
    public async Task<Result<ThemeManifestDto>> LoadManifestAsync(string themeId, CancellationToken cancellationToken)
    {
        string themePath = ResolveThemePath(themeId);
        if (!Directory.Exists(themePath))
            return Errors.Themes.ThemeFilesUnreadable;

        return await Task.FromResult(LoadManifestFromDirectory(themePath));
    }

    /// <summary>
    /// Gets the sanitized content of the template selected by a page key, falling back to the default template when the key is missing.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="pageKey">The page key that selects the template.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the template content, or an error.</returns>
    public async Task<Result<string>> GetTemplateAsync(string themeId, string pageKey, CancellationToken cancellationToken)
    {
        Result<ThemeManifestDto> manifestResult = await LoadManifestAsync(themeId, cancellationToken);
        if (manifestResult.IsFailure)
            return manifestResult.Errors;

        ThemeManifestDto manifest = manifestResult.Value;
        string themePath = ResolveThemePath(themeId);
        string? templatePath = manifest.Templates.TryGetValue(pageKey, out string? explicitPath)
            ? explicitPath
            : ResolveMirroredTemplatePath(themePath, pageKey);
        templatePath ??= manifest.Templates["default"]; // the manifest validation guarantees a default template

        Result<string> fullPathResult = ResolveContainedPath(themePath, templatePath);
        if (fullPathResult.IsFailure)
            return fullPathResult.Errors;

        if (!File.Exists(fullPathResult.Value))
            return Errors.Themes.ThemeFilesUnreadable;

        string template = await File.ReadAllTextAsync(fullPathResult.Value, cancellationToken);
        if (!AllowThemeScripts)
            template = StripScriptElements(template);

        return template;
    }

    /// <summary>
    /// Resolves the mirrored template path of a page key, walking up the scopes of the path when the exact mirror does not exist.
    /// </summary>
    /// <param name="themePath">The absolute path of the theme pack directory.</param>
    /// <param name="pageKey">The page key that selects the template.</param>
    /// <returns>The mirrored template path, or <see langword="null"/> when no mirrored template exists.</returns>
    private static string? ResolveMirroredTemplatePath(string themePath, string pageKey)
    {
        string? candidate = pageKey;
        while (candidate is not null)
        {
            // normalize the mirrored candidate so that malicious page keys cannot escape the theme pack
            Result<string> normalizedResult = NormalizeRelativePath($"templates/{candidate}.html");
            if (normalizedResult.IsSuccess)
            {
                Result<string> fullPathResult = ResolveContainedPath(themePath, normalizedResult.Value);
                if (fullPathResult.IsSuccess && File.Exists(fullPathResult.Value))
                    return normalizedResult.Value;
            }

            int lastSlash = candidate.LastIndexOf('/');
            candidate = lastSlash < 0 ? null : candidate[..lastSlash];
        }

        return null;
    }

    /// <summary>
    /// Gets a theme asset file.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="assetPath">The asset path relative to the theme pack root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the asset, or an error.</returns>
    public async Task<Result<ThemeAssetDto>> GetAssetAsync(string themeId, string assetPath, CancellationToken cancellationToken)
    {
        Result<string> normalizedResult = NormalizeRelativePath(assetPath);
        if (normalizedResult.IsFailure)
            return normalizedResult.Errors;

        string normalizedPath = normalizedResult.Value;
        if (!normalizedPath.StartsWith("assets/", StringComparison.Ordinal))
            return PackageInvalid("The requested asset is not located under the assets directory.");

        // scripts in theme assets are gated globally: when disabled, script files are not served at all
        if (!AllowThemeScripts && string.Equals(Path.GetExtension(normalizedPath), ".js", StringComparison.OrdinalIgnoreCase))
            return Errors.Themes.ThemeFilesUnreadable;

        Result<string> fullPathResult = ResolveContainedPath(ResolveThemePath(themeId), normalizedPath);
        if (fullPathResult.IsFailure)
            return fullPathResult.Errors;

        if (!File.Exists(fullPathResult.Value))
            return Errors.Themes.ThemeFilesUnreadable;

        byte[] bytes = await File.ReadAllBytesAsync(fullPathResult.Value, cancellationToken);
        return new ThemeAssetDto(bytes, ThemeMimeTypes.GetMimeType(fullPathResult.Value));
    }

    /// <summary>
    /// Builds a downloadable ZIP archive of an installed theme pack.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the archive, or an error.</returns>
    public async Task<Result<ThemeArchiveDto>> BuildArchiveAsync(string themeId, CancellationToken cancellationToken)
    {
        string themePath = ResolveThemePath(themeId);
        if (!Directory.Exists(themePath))
            return Errors.Themes.ThemeFilesUnreadable;

        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (string filePath in Directory.EnumerateFiles(themePath, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
            {
                string relativePath = Path.GetRelativePath(themePath, filePath).Replace(Path.DirectorySeparatorChar, '/');
                ZipArchiveEntry entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using FileStream input = File.OpenRead(filePath);
                await using Stream outputStream = entry.Open();
                await input.CopyToAsync(outputStream, cancellationToken);
            }
        }

        return new ThemeArchiveDto(output.ToArray(), $"{themeId}.zip", "application/zip");
    }

    /// <summary>
    /// Restores the files of a bundled theme from its shipped archive, used when the stored files were removed externally.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> RestoreBundledThemeAsync(string themeId, CancellationToken cancellationToken)
    {
        foreach (string archivePath in GetBundledThemeArchivePaths())
        {
            Result<ThemeManifestDto> manifestResult = await ReadManifestFromArchiveAsync(archivePath, cancellationToken);
            if (manifestResult.IsFailure)
                continue;

            if (!string.Equals(manifestResult.Value.Id, themeId, StringComparison.OrdinalIgnoreCase))
                continue;

            await using FileStream archive = File.OpenRead(archivePath);
            Result<ThemeManifestDto> installResult = await InstallAsync(archive, cancellationToken);
            if (installResult.IsFailure)
                return installResult.Errors;

            return Result.Success;
        }

        return Errors.Themes.ThemeFilesUnreadable;
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
            return PackageInvalid("The uploaded theme archive is empty.");

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
            return PackageInvalid("The theme archive contains no files.");

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
                return normalizedPathResult.Errors;

            string normalizedPath = normalizedPathResult.Value;
            if (!seenPaths.Add(normalizedPath))
                return PackageInvalid($"The archive contains a duplicate path: {normalizedPath}");

            bool isDirectory = entry.FullName.EndsWith('/') || string.IsNullOrEmpty(entry.Name);
            if (isDirectory)
            {
                Result<Success> directoryValidationResult = ValidatePackageDirectory(normalizedPath);
                if (directoryValidationResult.IsFailure)
                    return directoryValidationResult.Errors;

                Result<string> directoryPathResult = ResolveContainedPath(extractionPath, normalizedPath);
                if (directoryPathResult.IsFailure)
                    return directoryPathResult.Errors;

                Directory.CreateDirectory(directoryPathResult.Value);
                continue;
            }

            Result<Success> fileValidationResult = ValidatePackageFile(normalizedPath);
            if (fileValidationResult.IsFailure)
                return fileValidationResult.Errors;

            if (entry.Length > _options.MaxSingleFileBytes)
                return Error.Validation(code: "Theme.File.TooLarge", description: $"Theme file '{normalizedPath}' is too large.");

            // guard the total decompressed size, so a tiny archive cannot expand into a decompression bomb
            expandedBytes += entry.Length;
            if (expandedBytes > _options.MaxExpandedBytes)
                return Error.Validation(code: "Theme.Archive.ExpandedTooLarge", description: "The expanded theme archive is too large.");

            Result<string> outputPathResult = ResolveContainedPath(extractionPath, normalizedPath);
            if (outputPathResult.IsFailure)
                return outputPathResult.Errors;

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

                // enforce the per-file limit while streaming, because the declared entry size is not trustworthy
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
    /// Loads and validates the manifest of a theme pack from its directory.
    /// </summary>
    /// <param name="themeRoot">The absolute path of the theme pack directory.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest, or an error.</returns>
    private static Result<ThemeManifestDto> LoadManifestFromDirectory(string themeRoot)
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
            json = File.ReadAllText(manifestPath);
            manifest = JsonSerializer.Deserialize<ThemeManifestDto>(json, s_manifestJsonOptions);
        }
        catch (JsonException)
        {
            return Error.Validation(code: "Theme.Manifest.InvalidJson", description: "theme.json is not valid JSON.");
        }
        catch (IOException)
        {
            return Errors.Themes.ThemeFilesUnreadable;
        }

        if (manifest is null)
            return Error.Validation(code: "Theme.Manifest.InvalidJson", description: "theme.json contains no manifest object.");

        Result<Success> validationResult = ValidateManifest(themeRoot, manifest);
        if (validationResult.IsFailure)
            return validationResult.Errors;

        return manifest;
    }

    /// <summary>
    /// Deserializes a theme pack manifest from its JSON content.
    /// </summary>
    /// <param name="json">The JSON content of the manifest.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest, or an error.</returns>
    private static Result<ThemeManifestDto> DeserializeManifest(string json)
    {
        ThemeManifestDto? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<ThemeManifestDto>(json, s_manifestJsonOptions);
        }
        catch (JsonException)
        {
            return Error.Validation(code: "Theme.Manifest.InvalidJson", description: "theme.json is not valid JSON.");
        }

        if (manifest is null)
            return Error.Validation(code: "Theme.Manifest.InvalidJson", description: "theme.json contains no manifest object.");

        return manifest;
    }

    /// <summary>
    /// Validates the manifest fields and template mappings, normalizing the referenced paths.
    /// </summary>
    /// <param name="themeRoot">The absolute path of the theme pack directory.</param>
    /// <param name="manifest">The manifest to validate.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> ValidateManifest(string themeRoot, ThemeManifestDto manifest)
    {
        if (manifest.SchemaVersion != 1)
            return PackageInvalid("Only theme schemaVersion 1 is supported.");

        if (string.IsNullOrWhiteSpace(manifest.Id) || !s_themeIdPattern.IsMatch(manifest.Id) || manifest.Id.Length > 64)
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
                return normalizedPathResult.Errors;

            string normalizedPath = normalizedPathResult.Value;
            if (!normalizedPath.StartsWith("templates/", StringComparison.Ordinal))
                return PackageInvalid($"Template '{key}' must point to a file under templates/.");

            Result<string> fullTemplatePathResult = ResolveContainedPath(themeRoot, normalizedPath);
            if (fullTemplatePathResult.IsFailure)
                return fullTemplatePathResult.Errors;

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
                return previewPathResult.Errors;

            string previewPath = previewPathResult.Value;
            Result<string> fullPreviewPathResult = ResolveContainedPath(themeRoot, previewPath);
            if (fullPreviewPathResult.IsFailure)
                return fullPreviewPathResult.Errors;

            if (!previewPath.StartsWith("assets/", StringComparison.Ordinal) || !File.Exists(fullPreviewPathResult.Value))
                return PackageInvalid("The preview must reference an existing asset file.");

            manifest.Preview = previewPath;
        }

        return Result.Success;
    }

    /// <summary>
    /// Validates that a package file resides in the manifest, template or asset locations.
    /// </summary>
    /// <param name="normalizedPath">The normalized package-relative path of the file.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> ValidatePackageFile(string normalizedPath)
    {
        if (string.Equals(normalizedPath, MANIFEST_FILE_NAME, StringComparison.Ordinal))
            return Result.Success;

        if (normalizedPath.StartsWith("templates/", StringComparison.Ordinal))
            return Result.Success;

        if (normalizedPath.StartsWith("assets/", StringComparison.Ordinal))
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
    /// Creates a validation error for an invalid theme pack.
    /// </summary>
    /// <param name="description">The human-readable description of the package error.</param>
    /// <returns>The validation error describing the invalid theme pack.</returns>
    private static Error PackageInvalid(string description)
    {
        return Error.Validation(code: "Theme.Package.Invalid", description: description);
    }

    /// <summary>
    /// Removes every script element from a theme template, so that no script bundled in the theme runs when scripts are disabled.
    /// </summary>
    /// <param name="template">The template content to strip script elements from.</param>
    /// <returns>The template content without script elements.</returns>
    private static string StripScriptElements(string template)
    {
        return s_scriptElementPattern.Replace(template, string.Empty);
    }

    /// <summary>
    /// Resolves the storage path of a theme pack directory for a theme identifier.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <returns>The absolute path of the theme pack directory.</returns>
    private string ResolveThemePath(string themeId)
    {
        return Path.Combine(_storageRoot, themeId);
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
    /// Moves an extracted theme pack into the storage directory, replacing an existing pack with the same manifest id.
    /// </summary>
    /// <remarks>
    /// The move is retried on transient file system errors, because a concurrent process may briefly hold a file of the
    /// destination directory open (for example another host serving a template while this installation replaces the pack).
    /// Without the retry, that transient contention aborts the install and, when triggered by the startup job, crashes the
    /// whole host.
    /// </remarks>
    /// <param name="extractionPath">The directory of the extracted theme pack in the staging area.</param>
    /// <param name="destination">The storage directory the theme pack is moved to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task SwapIntoDestinationAsync(string extractionPath, string destination, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                if (Directory.Exists(destination))
                    Directory.Delete(destination, recursive: true);

                Directory.Move(extractionPath, destination);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(100 * attempt, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Resolves a configured path to an absolute path, rooted against the base directory when relative.
    /// </summary>
    /// <param name="basePath">The base directory the path is rooted against when relative.</param>
    /// <param name="configuredPath">The path from the configuration.</param>
    /// <returns>The absolute path.</returns>
    private static string ResolvePath(string basePath, string configuredPath)
    {
        return Path.GetFullPath(Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(basePath, configuredPath));
    }
}
