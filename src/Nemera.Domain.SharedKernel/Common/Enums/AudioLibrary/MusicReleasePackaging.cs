#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the outermost physical packaging of a music release.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicReleasePackaging
{
    /// <summary>
    /// The release is a book with a sleeve containing a medium, usually a compact disc.
    /// </summary>
    Book,

    /// <summary>
    /// The release is a box with a lid or an opening that contains the medium and other packaging, like posters and a booklet.
    /// </summary>
    Box,

    /// <summary>
    /// The release is a sleeve made of paper, paperboard, or cardboard.
    /// </summary>
    CardboardPaperSleeve,

    /// <summary>
    /// The release is a regular plastic case for a cassette.
    /// </summary>
    CassetteCase,

    /// <summary>
    /// The release is a minimalistic plastic case where the back and the front are kept together by a small hinge.
    /// </summary>
    ClamshellCase,

    /// <summary>
    /// The release is a bounded booklet, usually in hardcover, with a sleeve bound to its spine that houses a compact disc.
    /// </summary>
    Digibook,

    /// <summary>
    /// The release is a folded cardboard compact disc packaging that has a small pocket on one side for the disc.
    /// </summary>
    Digifile,

    /// <summary>
    /// The release is a folded case, typically made of coated paperboard, with one or more plastic trays glued into it.
    /// </summary>
    Digipak,

    /// <summary>
    /// The release is a pouch-like package with an internal mechanism that pushes the contents out when the lid flap is opened.
    /// </summary>
    DiscboxSlider,

    /// <summary>
    /// The release is a double-sided, double-width jewel case normally holding two to four compact discs.
    /// </summary>
    Fatbox,

    /// <summary>
    /// The release is a cardboard sleeve that folds in halves, thirds, or more, and can hold multiple records, compact discs, booklets, or posters.
    /// </summary>
    GatefoldCover,

    /// <summary>
    /// The release is the traditional compact disc case, made of hard, brittle plastic.
    /// </summary>
    JewelCase,

    /// <summary>
    /// The release is the traditional DVD case, made of soft plastic with a thin transparent cover protecting the artwork.
    /// </summary>
    KeepCase,

    /// <summary>
    /// The release is a large cardboard box, often used to sell compact discs in North America so that they would fit vinyl racks.
    /// </summary>
    Longbox,

    /// <summary>
    /// The release is an often decorated metal tin containing one or more compact discs.
    /// </summary>
    MetalTin,

    /// <summary>
    /// The release is a sleeve made entirely of plastic that holds the medium and the other parts of the album.
    /// </summary>
    PlasticSleeve,

    /// <summary>
    /// The release is a box with openings at its two ends that contains a tray holding the medium.
    /// </summary>
    Slidepack,

    /// <summary>
    /// The release is a thinner jewel case, commonly used for compact disc singles.
    /// </summary>
    SlimJewelCase,

    /// <summary>
    /// The release is a four or five-sided box that surrounds a second inner package such as a jewel case.
    /// </summary>
    Slipcase,

    /// <summary>
    /// The release is a digipak-like case held together with a snapping plastic closure.
    /// </summary>
    SnapCase,

    /// <summary>
    /// The release is a cardboard jacket, commonly used for 8cm compact disc singles in Japan.
    /// </summary>
    SnapPack,

    /// <summary>
    /// The release is a case similar to the regular jewel case, but with rounded corners and a latch closing mechanism.
    /// </summary>
    SuperJewelBox,

    /// <summary>
    /// The packaging of the release does not fit into any of the known categories.
    /// </summary>
    Other,

    /// <summary>
    /// The release has no packaging at all, which is common for digital media.
    /// </summary>
    None
}
