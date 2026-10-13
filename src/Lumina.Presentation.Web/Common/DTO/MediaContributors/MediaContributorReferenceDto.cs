#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaContributors;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaContributors;

/// <summary>
/// Data transfer object for the reference to a media contributor in a media item, carrying the role the contributor played.
/// </summary>
[DebuggerDisplay("{ContributorId} - {Role}")]
public class MediaContributorReferenceDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the media contributor.
    /// </summary>
    public Guid ContributorId { get; set; }

    /// <summary>
    /// Gets or sets the role the contributor played.
    /// </summary>
    public MediaContributorRole? Role { get; set; }
}
