#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Service for installing plugins from an uploaded archive into the plugin storage directory. The installed plugin assemblies are loaded by the host application at the next startup.
/// </summary>
internal sealed class PluginInstaller : IPluginInstaller
{
    // the install mutates the shared plugins directory, so the operations are serialized per resolved directory.
    // A per-instance gate is NOT enough: several hosts can run against the same plugins directory at once (for
    // example the parallel in-memory hosts started by the test factories), so the gate is keyed on the directory.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_pluginDirectoryGates = new(StringComparer.Ordinal);
    private readonly string _pluginsDirectory;
    private readonly SemaphoreSlim _mutationGate;
    private readonly ILogger<PluginInstaller> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstaller"/> class.
    /// </summary>
    /// <param name="pluginsSettings">The injected plugins configuration options.</param>
    /// <param name="logger">The logger for this service.</param>
    public PluginInstaller(IOptions<PluginsSettingsDto> pluginsSettings, ILogger<PluginInstaller> logger)
    {
        _pluginsDirectory = Path.Combine(AppContext.BaseDirectory, pluginsSettings.Value.Directory);
        _mutationGate = s_pluginDirectoryGates.GetOrAdd(_pluginsDirectory, static _ => new SemaphoreSlim(1, 1));
        _logger = logger;
    }

    /// <summary>
    /// Installs the plugin from the provided archive, placing its assemblies into the plugin storage directory.
    /// </summary>
    /// <param name="archive">The archive stream of the uploaded plugin.</param>
    /// <param name="fileName">The file name of the uploaded plugin.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> InstallAsync(Stream archive, string fileName, CancellationToken cancellationToken)
    {
        await _mutationGate.WaitAsync(cancellationToken);
        try
        {
            string extension = Path.GetExtension(fileName);
            if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                string? safeFileName = GetSafeFileName(fileName);
                if (safeFileName is null)
                    return Errors.Plugins.PluginFileNameCannotBeEmpty;
                await CopySingleAssemblyAsync(archive, safeFileName, cancellationToken).ConfigureAwait(false);
                return Result.Success;
            }

            if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return await ExtractZipArchiveAsync(archive, cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidDataException exception)
                {
                    _logger.LogWarning(exception, "An uploaded plugin archive could not be read.");
                    return Errors.Plugins.PluginArchiveNotReadable;
                }
            }

            return Errors.Plugins.UnsupportedPluginFileType;
        }
        finally
        {
            _mutationGate.Release();
        }
    }

    /// <summary>
    /// Copies a single plugin assembly into the plugin storage directory.
    /// </summary>
    /// <param name="assemblyStream">The stream of the plugin assembly.</param>
    /// <param name="assemblyFileName">The file name of the plugin assembly.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task CopySingleAssemblyAsync(Stream assemblyStream, string assemblyFileName, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_pluginsDirectory);
        string destinationPath = Path.Combine(_pluginsDirectory, assemblyFileName);
        await using FileStream outputStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await assemblyStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts the plugin assemblies from a ZIP archive into the plugin storage directory, flattening the archive
    /// structure, since the plugin loader scans the storage directory root for assemblies.
    /// </summary>
    /// <param name="archive">The stream of the uploaded ZIP archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ExtractZipArchiveAsync(Stream archive, CancellationToken cancellationToken)
    {
        using ZipArchive zipArchive = new(archive);
        Directory.CreateDirectory(_pluginsDirectory);
        bool foundAssembly = false;
        foreach (ZipArchiveEntry entry in zipArchive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? safeFileName = GetSafeFileName(entry.FullName);
            if (safeFileName is null || !string.Equals(Path.GetExtension(safeFileName), ".dll", StringComparison.OrdinalIgnoreCase))
                continue;
            foundAssembly = true;
            string destinationPath = Path.Combine(_pluginsDirectory, safeFileName);
            await using Stream entryStream = entry.Open();
            await using FileStream outputStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await entryStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);
        }

        if (!foundAssembly)
            return Errors.Plugins.PluginArchiveContainsNoAssemblies;
        return Result.Success;
    }

    /// <summary>
    /// Extracts the bare file name of an uploaded plugin file or archive entry, stripping every directory component,
    /// so that a path traversal attempt cannot write outside the plugin storage directory on any platform.
    /// </summary>
    /// <param name="path">The file path to sanitize.</param>
    /// <returns>The bare file name, or <see langword="null"/> when the path contains no usable file name.</returns>
    private static string? GetSafeFileName(string path)
    {
        // a backslash is not a directory separator on non-Windows platforms, so it is normalized to '/' first,
        // after which Path.GetFileName can strip any directory component on every platform
        string fileName = Path.GetFileName(path.Replace('\\', '/'));
        return fileName is "" or "." or ".." ? null : fileName;
    }
}
