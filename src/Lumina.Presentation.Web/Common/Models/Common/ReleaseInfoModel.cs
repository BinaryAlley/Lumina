#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Represents a request to get release information.
/// </summary>
[DebuggerDisplay("{OriginalReleaseDate}")]
public class ReleaseInfoModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the optional original release date of the media item.
    /// </summary>
    public DateOnly? OriginalReleaseDate { get; set; }

    /// <summary>
    /// Gets the optional original release year of the media item.
    /// This is useful when only the year of original release is known.
    /// </summary>
    public int? OriginalReleaseYear { get; set; }

    /// <summary>
    /// Gets the optional re-release or reissue date of the media item.
    /// </summary>
    public DateOnly? ReReleaseDate { get; set; }

    /// <summary>
    /// Gets the optional re-release or reissue year of the media item.
    /// </summary>
    public int? ReReleaseYear { get; set; }

    /// <summary>
    /// Gets the optional country or region of release.
    /// </summary>
    public string? ReleaseCountry { get; set; }

    /// <summary>
    /// Gets the optional release version or edition. (e.g. "Original", "Director's Cut", "2.0")
    /// </summary>
    public string? ReleaseVersion { get; set; }
    #endregion
}