#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.Favorites;

/// <summary>
/// Enumeration for the kinds of media item that can be marked as a favorite.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum FavoriteMediaKind
{
    /// <summary>
    /// No media kind. Used as the default value; it is not a kind that can be marked as a favorite.
    /// </summary>
    None = 0,

    /// <summary>
    /// A favorite that references an artist of a music library.
    /// </summary>
    MusicArtist,

    /// <summary>
    /// A favorite that references an album of a music library.
    /// </summary>
    MusicAlbum,

    /// <summary>
    /// A favorite that references a track of a music library.
    /// </summary>
    MusicTrack,

    /// <summary>
    /// A favorite that references a book of a book library.
    /// </summary>
    Book,

    /// <summary>
    /// A favorite that references a video file. Planned: video libraries are not catalogued yet, so this kind cannot be favorited.
    /// </summary>
    Video,

    /// <summary>
    /// A favorite that references a photo. Planned: photo libraries are not catalogued yet, so this kind cannot be favorited.
    /// </summary>
    Photo,
}
