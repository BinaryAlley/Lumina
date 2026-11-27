#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the quality cap applied to the audio the server normalizes (remuxes or transcodes) for a user.
/// </summary>
/// <remarks>
/// The caps apply only to audio the server re-encodes into a lossy codec. A track that is played directly is delivered as it was stored, a remux only
/// repackages the stored codec without re-encoding it, and a lossless transcode is not reduced, so none of those deliveries is capped.
/// </remarks>
[DebuggerDisplay("{ToString()}")]
public enum PlaybackStreamingQuality
{
    /// <summary>
    /// The server chooses the quality, with a default cap of 192 kbps for a lossy re-encode.
    /// </summary>
    Auto,

    /// <summary>
    /// A lossy normalized audio stream is capped at 96 kbps.
    /// </summary>
    Low,

    /// <summary>
    /// A lossy normalized audio stream is capped at 160 kbps.
    /// </summary>
    Medium,

    /// <summary>
    /// A lossy normalized audio stream is capped at 320 kbps.
    /// </summary>
    High,
}
