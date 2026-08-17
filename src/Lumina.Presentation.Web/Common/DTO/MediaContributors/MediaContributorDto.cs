#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaContributors;

/// <summary>
/// Data transfer object for a media contributor.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MediaContributorDto
{
    /// <summary>
    /// Gets the name of the contributor.
    /// </summary>
    public MediaContributorNameDto? Name { get; set; }

    /// <summary>
    /// Gets the role of the contributor.
    /// </summary>
    public MediaContributorRoleDto? Role { get; set; }
}
