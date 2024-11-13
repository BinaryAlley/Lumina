#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaContributors;

/// <summary>
/// Repository entity for a media contributor role.
/// </summary>
/// <param name="Name">The name of the role assigned to the media contributor.</param>
/// <param name="Category">The category of the role.</param>
[DebuggerDisplay("Name: {Name}, Category: {Category}")]
public record MediaContributorRoleEntity(
    string? Name,
    string? Category
);
