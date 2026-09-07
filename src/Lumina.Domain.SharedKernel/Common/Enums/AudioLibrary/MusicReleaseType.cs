#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the type of a music release, describing the format of the release in terms of its content.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicReleaseType
{
    /// <summary>
    /// A release consisting of a collection of tracks.
    /// </summary>
    Album,

    /// <summary>
    /// A release that was aired as a broadcast.
    /// </summary>
    Broadcast,

    /// <summary>
    /// A release that is longer than a single but shorter than an album.
    /// </summary>
    Ep,

    /// <summary>
    /// The type of the release does not fall into any of the known categories.
    /// </summary>
    Other,

    /// <summary>
    /// A release consisting of a small number of tracks.
    /// </summary>
    Single,

    /// <summary>
    /// A release containing a dramatized audio performance.
    /// </summary>
    AudioDrama,

    /// <summary>
    /// A release containing a recording of a book being read aloud.
    /// </summary>
    AudioBook,

    /// <summary>
    /// A release that gathers tracks from various sources or artists.
    /// </summary>
    Compilation,

    /// <summary>
    /// A release containing a continuous mix of tracks by a DJ.
    /// </summary>
    DjMix,

    /// <summary>
    /// A release of demo recordings, typically distributed before the official release.
    /// </summary>
    Demo,

    /// <summary>
    /// A release containing an audio recording made outside of a studio.
    /// </summary>
    FieldRecording,

    /// <summary>
    /// A release containing a recorded interview.
    /// </summary>
    Interview,

    /// <summary>
    /// A release recorded during a live performance.
    /// </summary>
    Live,

    /// <summary>
    /// A release containing a compilation of tracks, typically distributed outside of official channels.
    /// </summary>
    Mixtape,

    /// <summary>
    /// A release containing modified or reinterpreted versions of original recordings.
    /// </summary>
    Remix,

    /// <summary>
    /// A release containing music written for a motion picture or television program.
    /// </summary>
    Soundtrack,

    /// <summary>
    /// A release containing spoken word content.
    /// </summary>
    SpokenWord
}
