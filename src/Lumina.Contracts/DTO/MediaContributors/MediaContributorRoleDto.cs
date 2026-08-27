#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaContributors;

/// <summary>
/// Data transfer object for the role of a media contributor in a media item.
/// </summary>
/// <param name="Name">The display name of the role assigned to the media contributor.</param>
/// <param name="Category">The canonical category of the role.</param>
[DebuggerDisplay("Name: {Name}, Category: {Category}")]
public record MediaContributorRoleDto(
    string? Name,
    MediaContributorRoleCategory? Category
);
