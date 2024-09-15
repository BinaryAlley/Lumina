#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Represents a request to get tag information.
/// </summary>
[DebuggerDisplay("{Name}")]
public class TagModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the tag element of the media item.
    /// </summary>
    public string? Name { get; set; }
    #endregion
}