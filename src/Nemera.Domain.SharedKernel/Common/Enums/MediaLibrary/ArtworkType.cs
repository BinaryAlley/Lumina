#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the type of artwork of a media library item.
/// Each media library item can have multiple artwork of different types, and multiple instances of the same
/// type (like the booklet pages of an audio album), tracked independently so that a change of one artwork
/// does not require the others to be re-fetched.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum ArtworkType
{
    /// <summary>
    /// The main artwork of the media library item, like the cover of a book.
    /// </summary>
    Cover,

    /// <summary>
    /// The front of the artwork of the media library item, when it is stored separately from the cover.
    /// </summary>
    Front,

    /// <summary>
    /// The back of the artwork of the media library item.
    /// </summary>
    Back,

    /// <summary>
    /// A page of the booklet of the media library item.
    /// </summary>
    Booklet,

    /// <summary>
    /// The artwork printed on the medium of the media library item, like a disc label.
    /// </summary>
    Medium,

    /// <summary>
    /// The artwork printed on the tray of the media library item.
    /// </summary>
    Tray,

    /// <summary>
    /// The artwork printed on the spine of the media library item.
    /// </summary>
    Spine,

    /// <summary>
    /// The obi strip of the media library item.
    /// </summary>
    Obi,

    /// <summary>
    /// The sticker of the media library item.
    /// </summary>
    Sticker,

    /// <summary>
    /// The poster of the media library item.
    /// </summary>
    Poster,

    /// <summary>
    /// The liner notes of the media library item.
    /// </summary>
    Liner,

    /// <summary>
    /// A watermark carried by the artwork of the media library item.
    /// </summary>
    Watermark,

    /// <summary>
    /// The backdrop, or fanart, of the media library item.
    /// </summary>
    Backdrop,

    /// <summary>
    /// The banner of the media library item.
    /// </summary>
    Banner,

    /// <summary>
    /// The logo of the media library item.
    /// </summary>
    Logo,

    /// <summary>
    /// The thumbnail of the media library item.
    /// </summary>
    Thumb,

    /// <summary>
    /// Any other artwork of the media library item.
    /// </summary>
    Other
}
