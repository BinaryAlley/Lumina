#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaContributors;

/// <summary>
/// Repository entity for a media contributor.
/// </summary>
/// <param name="Name">The name details of the media contributor, including display and legal names.</param>
/// <param name="Role">The role details of the media contributor, including the role name and category.</param>
[DebuggerDisplay("Name: {Name}, Role: {Role}")]
public record MediaContributorEntity(
    MediaContributorNameEntity? Name,
    MediaContributorRoleEntity? Role
);
