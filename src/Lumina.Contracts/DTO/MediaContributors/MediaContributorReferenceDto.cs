#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaContributors;

/// <summary>
/// Data transfer object for the reference to a media contributor in a media item, carrying the role the contributor played.
/// </summary>
/// <param name="ContributorId">The unique identifier of the media contributor.</param>
/// <param name="Role">The role the contributor played.</param>
[DebuggerDisplay("{ContributorId} - {Role}")]
public record MediaContributorReferenceDto(
    Guid ContributorId,
    MediaContributorRole Role
);
