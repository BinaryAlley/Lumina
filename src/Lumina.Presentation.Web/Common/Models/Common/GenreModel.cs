#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Represents a request to get genre information.
/// </summary>
[DebuggerDisplay("{Name}")]
public class GenreModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the genre element of the media item.
    /// </summary>
    public string? Name { get; set; }
    #endregion
}