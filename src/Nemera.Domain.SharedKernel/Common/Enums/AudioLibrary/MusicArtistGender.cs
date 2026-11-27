#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the gender of a music artist, applicable only to artists that are persons or characters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicArtistGender
{
    /// <summary>
    /// The artist identifies as male.
    /// </summary>
    Male,

    /// <summary>
    /// The artist identifies as female.
    /// </summary>
    Female,

    /// <summary>
    /// The artist identifies with a gender other than male or female.
    /// </summary>
    Other,

    /// <summary>
    /// The gender of the artist is not applicable, or is deliberately not stated.
    /// </summary>
    NotApplicable
}
