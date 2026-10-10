#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the type of a music artist, describing whether the artist is an individual, an ensemble, or something else.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicArtistType
{
    /// <summary>
    /// The artist is an individual person.
    /// </summary>
    Person,

    /// <summary>
    /// The artist is a group of people that may or may not have a distinctive name.
    /// </summary>
    Group,

    /// <summary>
    /// The artist is an orchestra, a large instrumental ensemble.
    /// </summary>
    Orchestra,

    /// <summary>
    /// The artist is a choir or chorus, a large vocal ensemble.
    /// </summary>
    Choir,

    /// <summary>
    /// The artist is an individual fictional character.
    /// </summary>
    Character,

    /// <summary>
    /// The type of the artist does not fit into any of the known categories.
    /// </summary>
    Other
}
