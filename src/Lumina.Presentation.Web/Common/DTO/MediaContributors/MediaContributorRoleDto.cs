#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaContributors;

/// <summary>
/// Data transfer object for a media contributor role.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MediaContributorRoleDto
{
    /// <summary>
    /// Gets the value representing this object.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the category of this object.
    /// </summary>
    public string? Category { get; set; }
}
