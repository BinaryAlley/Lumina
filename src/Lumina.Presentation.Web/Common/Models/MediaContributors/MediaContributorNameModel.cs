#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.MediaContributors;

/// <summary>
/// Represents a media contributor name.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public class MediaContributorNameModel
{
    /// <summary>
    /// Gets the name by which the contributor is popularly known.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets the legal name of the contributor.
    /// </summary>
    public string? LegalName { get; set; }
}