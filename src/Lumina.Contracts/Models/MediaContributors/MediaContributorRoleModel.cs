#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.MediaContributors;

/// <summary>
/// Represents a media contributor role.
/// </summary>
/// <param name="Name">Gets the name of the role assigned to the media contributor.</param>
/// <param name="Category">Gets the category of the role, which may describe the type or group of roles.</param>
[DebuggerDisplay("{Name} {Category}")]
public record MediaContributorRoleModel(
    string? Name,
    string? Category
);