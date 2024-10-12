#region ========================================================================= USING =====================================================================================
using System.Diagnostics;

#endregion

namespace Lumina.Presentation.Web.Common.Models.MediaContributors;

/// <summary>
/// Represents a media contributor.
/// </summary>
[DebuggerDisplay("{Name}")]
public class MediaContributorModel
{
    /// <summary>
    /// Gets the name of the contributor.
    /// </summary>
    public MediaContributorNameModel? Name { get; set; }

    /// <summary>
    /// Gets the role of the contributor.
    /// </summary>
    public MediaContributorRoleModel? Role { get; set; }
}