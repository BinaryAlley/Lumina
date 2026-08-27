#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;

/// <summary>
/// Enumeration for the canonical category of the role of a media contributor in a media item.
/// The category normalizes the free-form role names returned by the metadata providers, so that roles
/// that describe the same kind of contribution, like "Author" and "Writer", are never treated as distinct.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MediaContributorRoleCategory
{
    /// <summary>
    /// The role does not fall into any of the known categories.
    /// </summary>
    Other,

    /// <summary>
    /// The contributor created the media item, like a book, a movie, or a piece of music.
    /// </summary>
    Author,

    /// <summary>
    /// The contributor translated the media item into another language.
    /// </summary>
    Translator,

    /// <summary>
    /// The contributor created the illustrations of the media item.
    /// </summary>
    Illustrator,

    /// <summary>
    /// The contributor published the media item.
    /// </summary>
    Publisher,

    /// <summary>
    /// The contributor narrated the media item.
    /// </summary>
    Narrator
}
