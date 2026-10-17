#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Caching;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Startup component that owns the normalized audio cache lifecycle and verifies the ffmpeg toolchain before any playback request is served.
/// </summary>
/// <remarks>
/// The cache is cleaned and recreated with private permissions at startup, and the configured ffmpeg binary is probed for the encoders the catalog uses. The
/// cleanup only removes the entries the cache itself owns, and only from a directory that carries the cache marker or was empty, so a configured directory
/// that belongs to something else is never wiped. The cleanup is bounded, so a slow or network backed cache directory cannot delay host readiness without
/// limit. The probe outcome is recorded in <see cref="TranscodingToolchainStatus"/>, so normalized playback reports that it is unavailable for the affected
/// formats instead of failing per track. A failure of either step is logged and does not abort the host, so direct playback of the rest of the application
/// keeps working.
/// </remarks>
internal sealed class TranscodingStartupInitializer : IHostedService
{
    private const int CACHE_CLEANUP_TIMEOUT_SECONDS = 30;
    private const int FFMPEG_PROBE_TIMEOUT_SECONDS = 10;

    private readonly TranscodingSettingsDto _settings;
    private readonly TranscodingToolchainStatus _toolchainStatus;
    private readonly ILogger<TranscodingStartupInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingStartupInitializer"/> class.
    /// </summary>
    /// <param name="options">The audio normalization configuration options.</param>
    /// <param name="toolchainStatus">The status that records the outcome of the ffmpeg probe for the normalization service.</param>
    /// <param name="logger">The logger for this startup component.</param>
    public TranscodingStartupInitializer(IOptions<TranscodingSettingsDto> options, TranscodingToolchainStatus toolchainStatus, ILogger<TranscodingStartupInitializer> logger)
    {
        _settings = options.Value;
        _toolchainStatus = toolchainStatus;
        _logger = logger;
    }

    /// <summary>
    /// Cleans and recreates the cache directory, then probes the ffmpeg binary for the encoders the pipeline uses.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous startup operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            string cacheDirectory = TranscodingCachePaths.GetRootDirectory(_settings.CacheDirectory);
            if (Directory.Exists(cacheDirectory))
            {
                // The permissions are verified before the directory is inspected, so a directory planted by another account is never used or cleaned.
                TranscodingCachePaths.RestrictPermissions(cacheDirectory);
                if (TranscodingCachePaths.HasCacheMarker(cacheDirectory))
                {
                    // Only the entries the cache itself owns are removed, so a directory that was configured by mistake can never lose unrelated data.
                    // The cleanup is bounded and cancellable, because a network backed cache directory must not delay host readiness forever.
                    using (CancellationTokenSource cleanupTimeoutCancellationTokenSource = new(TimeSpan.FromSeconds(CACHE_CLEANUP_TIMEOUT_SECONDS)))
                    {
                        using (CancellationTokenSource linkedCleanupCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cleanupTimeoutCancellationTokenSource.Token))
                        {
                            try
                            {
                                TranscodingCachePaths.ClearCacheEntries(cacheDirectory, linkedCleanupCancellationTokenSource.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                    _logger.LogWarning("The normalized audio cache cleanup did not finish within {CleanupTimeoutSeconds} seconds. Whatever remains is accounted for by the coordinator and is removed by the next cleanup.", CACHE_CLEANUP_TIMEOUT_SECONDS);
                            }
                        }
                    }
                }
                else if (!Directory.EnumerateFileSystemEntries(cacheDirectory).Any())
                    TranscodingCachePaths.WriteCacheMarker(cacheDirectory); // An empty directory is adopted, so a cache directory created ahead of the first run works without manual preparation.
                else // A non-empty directory without the marker was not created by this application. It is never wiped, because only the operator can tell whether its entries are safe to remove.                    
                    throw new IOException("The configured normalized audio cache directory exists, is not empty and does not carry the Lumina cache marker, so it was not wiped.");
            }
            else
                TranscodingCachePaths.EnsureDirectoryExists(cacheDirectory);
            _logger.LogInformation("The normalized audio cache uses the directory '{CacheDirectory}'.", cacheDirectory);
        }
        catch (Exception exception)
        {
            // A locked file, a read-only volume or a directory owned by another account must not stop the whole application; the coordinator accounts
            // for whatever remains and the service reports normalized playback as unavailable when the directory still cannot be used.
            _logger.LogError(exception, "The normalized audio cache could not be initialized (configured directory: '{CacheDirectory}'). Normalized playback may be unavailable.", _settings.CacheDirectory ?? "(default)");
        }

        try
        {
            await ProbeFfmpegAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // The startup component promises never to abort the host, so an unexpected probe failure is contained and reported here.
            _toolchainStatus.RecordUnavailable();
            _logger.LogError(exception, "The ffmpeg startup probe failed unexpectedly. Normalized playback may be unavailable.");
        }
    }

    /// <summary>
    /// Stops the startup component, which has no resources of its own to release.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Probes the ffmpeg binary for the encoders the catalog can produce, without failing the host when it is missing or incomplete.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous probe.</returns>
    private async Task ProbeFfmpegAsync(CancellationToken cancellationToken)
    {
        try
        {
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _settings.FfmpegPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.StartInfo.ArgumentList.Add("-hide_banner");
            process.StartInfo.ArgumentList.Add("-encoders");

            if (!process.Start())
            {
                _toolchainStatus.RecordUnavailable();
                _logger.LogWarning("The ffmpeg binary '{FfmpegPath}' could not be started. Normalized playback will be unavailable.", _settings.FfmpegPath);
                return;
            }

            using CancellationTokenSource timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(FFMPEG_PROBE_TIMEOUT_SECONDS));
            // The probe has two cancellation reasons: the host startup token and the probe timeout. Linking them gives the process wait and the pipe reads
            // one token that means "this probe is over" for either reason. The parameter alone could not stop the reads on a timeout, and the timeout alone would ignore host shutdown.
            using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);
            // Drain both pipes while the process runs. A child that fills a pipe buffer blocks until it is read, so waiting for exit first could deadlock.
            // The linked token stops the reads as well, so a probe timeout or host shutdown does not leave them running against a killed child.
            Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync(linkedCancellationTokenSource.Token);
            Task<string> standardErrorTask = process.StandardError.ReadToEndAsync(linkedCancellationTokenSource.Token);
            try
            {
                await process.WaitForExitAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch (Exception killException) when (killException is Win32Exception or InvalidOperationException or NotSupportedException)
                {
                    _logger.LogDebug(killException, "The ffmpeg startup probe process could not be stopped.");
                }
                if (!cancellationToken.IsCancellationRequested)
                    _logger.LogWarning("The ffmpeg binary '{FfmpegPath}' did not respond to the startup probe.", _settings.FfmpegPath);
                _toolchainStatus.RecordUnavailable();
                return;
            }

            string encoderListing = await standardOutputTask.ConfigureAwait(false);
            await standardErrorTask.ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                _toolchainStatus.RecordUnavailable();
                _logger.LogWarning("The ffmpeg binary '{FfmpegPath}' exited with code {ExitCode} during the startup probe. Normalized playback may be unavailable.", _settings.FfmpegPath, process.ExitCode);
                return;
            }

            IReadOnlyList<string> requiredEncoders = [.. PlaybackFormatCatalog.Formats
                .Select(format => format.EncoderName)
                .Distinct(StringComparer.OrdinalIgnoreCase)];
            List<string> missingEncoders = [.. requiredEncoders.Where(encoder => !ContainsEncoder(encoderListing, encoder))];
            if (missingEncoders.Count > 0)
            {
                // An unpinned distribution package can change the encoder set without changing the application build, so this is reported at startup instead of surfacing as a per track playback failure.
                _toolchainStatus.RecordMissingEncoders(missingEncoders);
                _logger.LogError("The ffmpeg binary '{FfmpegPath}' is missing the encoders {MissingEncoders}. Normalized playback for those formats will be unavailable.", _settings.FfmpegPath, string.Join(", ", missingEncoders));
                return;
            }
            _toolchainStatus.RecordAvailable();
            _logger.LogInformation("The ffmpeg binary '{FfmpegPath}' provides every encoder the normalized audio pipeline uses.", _settings.FfmpegPath);
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _toolchainStatus.RecordUnavailable();
            _logger.LogWarning(exception, "The ffmpeg binary '{FfmpegPath}' could not be probed. Normalized playback will be unavailable.", _settings.FfmpegPath);
        }
    }

    /// <summary>
    /// Checks whether the encoder listing of ffmpeg contains the provided encoder name.
    /// </summary>
    /// <param name="encoderListing">The output of the ffmpeg encoder listing.</param>
    /// <param name="encoderName">The encoder name to look for.</param>
    /// <returns><see langword="true"/> when the encoder is listed, otherwise <see langword="false"/>.</returns>
    private static bool ContainsEncoder(string encoderListing, string encoderName)
    {
        foreach (string line in encoderListing.Split('\n'))
        {
            string[] tokens = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            // An encoder line is "<flags> <name> <description>"; the encoder name is always the second token.
            if (tokens.Length >= 2 && string.Equals(tokens[1], encoderName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
