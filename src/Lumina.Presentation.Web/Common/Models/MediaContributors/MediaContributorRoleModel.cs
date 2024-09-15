#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.MediaContributors;

/// <summary>
/// Represents a media contributor role.
/// </summary>
[DebuggerDisplay("{Name}")]
public class MediaContributorRoleModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the value representing this object.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the category of this object.
    /// </summary>
    public string? Category { get; set; }
    #endregion
}