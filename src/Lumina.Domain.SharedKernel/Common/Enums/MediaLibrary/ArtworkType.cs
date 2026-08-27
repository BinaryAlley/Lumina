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
    Cover
}
