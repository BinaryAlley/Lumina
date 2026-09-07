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
    Narrator,

    /// <summary>
    /// The contributor is credited as the artist of the media item.
    /// </summary>
    Artist,

    /// <summary>
    /// The contributor performed the media item, whether singing or playing an instrument.
    /// </summary>
    Performer,

    /// <summary>
    /// The contributor wrote the music of the media item.
    /// </summary>
    Composer,

    /// <summary>
    /// The contributor wrote the lyrics of the media item.
    /// </summary>
    Lyricist,

    /// <summary>
    /// The contributor oversaw the production of the media item.
    /// </summary>
    Producer,

    /// <summary>
    /// The contributor operated the recording equipment during the production of the media item.
    /// </summary>
    Engineer,

    /// <summary>
    /// The contributor conducted the ensemble performing the media item.
    /// </summary>
    Conductor,

    /// <summary>
    /// The contributor arranged the music of the media item.
    /// </summary>
    Arranger,

    /// <summary>
    /// The contributor mixed the audio of the media item.
    /// </summary>
    Mixer,

    /// <summary>
    /// The contributor created a remix of the media item.
    /// </summary>
    Remixer,

    /// <summary>
    /// The contributor released the media item under a record label.
    /// </summary>
    RecordLabel
}
