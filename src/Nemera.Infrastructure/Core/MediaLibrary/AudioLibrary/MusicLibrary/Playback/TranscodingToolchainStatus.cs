#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Records the outcome of the ffmpeg startup probe, so a missing toolchain or encoder is reported as a retryable unavailability instead of a production failure.
/// </summary>
/// <remarks>
/// The status starts as available, because a request that arrives before the probe completes is better served by attempting the work than by rejecting it. The
/// probe replaces the status with its outcome, and every later production consults it before it spends a job slot.
/// </remarks>
internal sealed class TranscodingToolchainStatus
{
    // The startup probe writes these fields and request threads read them without a lock, so volatile makes each write visible to the readers.
    private volatile bool _isTranscoderAvailable = true;
    private volatile FrozenSet<string> _missingEncoders = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingToolchainStatus"/> class, which starts as available until the startup probe records its outcome.
    /// </summary>
    public TranscodingToolchainStatus()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the ffmpeg binary can run at all.
    /// </summary>
    public bool IsTranscoderAvailable => _isTranscoderAvailable;

    /// <summary>
    /// Records that the probe found a usable ffmpeg binary with every encoder the catalog needs.
    /// </summary>
    public void RecordAvailable()
    {
        // The set is cleared before the availability flag, so a reader never observes a missing encoder together with a fresh successful probe.
        _missingEncoders = [];
        _isTranscoderAvailable = true;
    }

    /// <summary>
    /// Records that the ffmpeg binary could not be started or did not answer the probe.
    /// </summary>
    public void RecordUnavailable()
    {
        _isTranscoderAvailable = false;
        _missingEncoders = [];
    }

    /// <summary>
    /// Records that ffmpeg runs, but that some of the encoders the catalog needs are missing.
    /// </summary>
    /// <param name="missingEncoders">The names of the encoders the startup probe did not find.</param>
    public void RecordMissingEncoders(IEnumerable<string> missingEncoders)
    {
        _missingEncoders = missingEncoders.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _isTranscoderAvailable = true;
    }

    /// <summary>
    /// Checks whether an encoder can be used for a lossy or lossless transcode.
    /// </summary>
    /// <param name="encoderName">The name of the ffmpeg encoder the delivery format needs.</param>
    /// <returns><see langword="true"/> when ffmpeg runs and the encoder was found, otherwise <see langword="false"/>.</returns>
    public bool IsEncoderAvailable(string encoderName)
    {
        return _isTranscoderAvailable && !_missingEncoders.Contains(encoderName);
    }
}
