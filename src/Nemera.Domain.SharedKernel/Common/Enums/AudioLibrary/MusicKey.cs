#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the musical key of a track, describing the pitch and the major or minor mode of the key.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicKey
{
    /// <summary>
    /// The musical key of C major.
    /// </summary>
    CMajor,

    /// <summary>
    /// The musical key of C sharp major.
    /// </summary>
    CSharpMajor,

    /// <summary>
    /// The musical key of D major.
    /// </summary>
    DMajor,

    /// <summary>
    /// The musical key of D sharp major.
    /// </summary>
    DSharpMajor,

    /// <summary>
    /// The musical key of E major.
    /// </summary>
    EMajor,

    /// <summary>
    /// The musical key of F major.
    /// </summary>
    FMajor,

    /// <summary>
    /// The musical key of F sharp major.
    /// </summary>
    FSharpMajor,

    /// <summary>
    /// The musical key of G major.
    /// </summary>
    GMajor,

    /// <summary>
    /// The musical key of G sharp major.
    /// </summary>
    GSharpMajor,

    /// <summary>
    /// The musical key of A major.
    /// </summary>
    AMajor,

    /// <summary>
    /// The musical key of A sharp major.
    /// </summary>
    ASharpMajor,

    /// <summary>
    /// The musical key of B major.
    /// </summary>
    BMajor,

    /// <summary>
    /// The musical key of C minor.
    /// </summary>
    CMinor,

    /// <summary>
    /// The musical key of C sharp minor.
    /// </summary>
    CSharpMinor,

    /// <summary>
    /// The musical key of D minor.
    /// </summary>
    DMinor,

    /// <summary>
    /// The musical key of D sharp minor.
    /// </summary>
    DSharpMinor,

    /// <summary>
    /// The musical key of E minor.
    /// </summary>
    EMinor,

    /// <summary>
    /// The musical key of F minor.
    /// </summary>
    FMinor,

    /// <summary>
    /// The musical key of F sharp minor.
    /// </summary>
    FSharpMinor,

    /// <summary>
    /// The musical key of G minor.
    /// </summary>
    GMinor,

    /// <summary>
    /// The musical key of G sharp minor.
    /// </summary>
    GSharpMinor,

    /// <summary>
    /// The musical key of A minor.
    /// </summary>
    AMinor,

    /// <summary>
    /// The musical key of A sharp minor.
    /// </summary>
    ASharpMinor,

    /// <summary>
    /// The musical key of B minor.
    /// </summary>
    BMinor
}
