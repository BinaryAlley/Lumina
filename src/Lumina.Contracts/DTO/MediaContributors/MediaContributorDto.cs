#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaContributors;

/// <summary>
/// Data transfer object for a media contributor.
/// </summary>
/// <param name="Name">The name details of the media contributor, including display and legal names.</param>
/// <param name="Role">The canonical role of the media contributor in the media item, used as the key of its localized display string.</param>
[DebuggerDisplay("Name: {Name}, Role: {Role}")]
public record MediaContributorDto(
    MediaContributorNameDto? Name,
    MediaContributorRole? Role
);
