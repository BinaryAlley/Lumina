#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaContributors;

/// <summary>
/// Data transfer object for a media contributor role.
/// </summary>
/// <param name="Name">The name of the role assigned to the media contributor.</param>
/// <param name="Category">The category of the role.</param>
[DebuggerDisplay("Name: {Name}, Category: {Category}")]
public record MediaContributorRoleDto(
    string? Name,
    string? Category
);
