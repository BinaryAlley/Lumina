#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.MediaContributors;

/// <summary>
/// Represents a media contributor.
/// </summary>
/// <param name="Name">Gets the name details of the media contributor, including display and legal names.</param>
/// <param name="Role">Gets the role details of the media contributor, including the role name and category.</param>
[DebuggerDisplay("{Name} {Role}")]
public record MediaContributorModel(
    MediaContributorNameModel? Name,
    MediaContributorRoleModel? Role
);