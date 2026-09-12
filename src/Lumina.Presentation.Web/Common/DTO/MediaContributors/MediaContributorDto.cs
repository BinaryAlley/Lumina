#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaContributors;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaContributors;

/// <summary>
/// Data transfer object for a media contributor.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MediaContributorDto
{
    /// <summary>
    /// Gets the name of the contributor.
    /// </summary>
    public MediaContributorNameDto? Name { get; set; }

    /// <summary>
    /// Gets the canonical role of the contributor, used as the key of its localized display string.
    /// </summary>
    public MediaContributorRole? Role { get; set; }
}
