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
using System.Linq;
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
                await CopySingleAssemblyAsync(archive, Path.GetFileName(fileName), cancellationToken).ConfigureAwait(false);
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
        ZipArchiveEntry[] assemblyEntries = [.. zipArchive.Entries.Where(entry => string.Equals(Path.GetExtension(entry.FullName), ".dll", StringComparison.OrdinalIgnoreCase))];
        if (assemblyEntries.Length == 0)
            return Errors.Plugins.PluginArchiveContainsNoAssemblies;

        Directory.CreateDirectory(_pluginsDirectory);
        foreach (ZipArchiveEntry entry in assemblyEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // flatten the archive structure and use only the file name, so no entry can escape the plugin storage directory
            string destinationPath = Path.Combine(_pluginsDirectory, Path.GetFileName(entry.FullName));
            await using Stream entryStream = entry.Open();
            await using FileStream outputStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await entryStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);
        }

        return Result.Success;
    }
}
