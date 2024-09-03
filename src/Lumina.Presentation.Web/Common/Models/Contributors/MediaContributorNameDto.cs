namespace Lumina.Presentation.Web.Common.Models.Contributors;

/// <summary>
/// Represents a media contributor name.
/// </summary>
public class MediaContributorNameDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name by which the contributor is popularly known.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets the legal name of the contributor.
    /// </summary>
    public string? LegalName { get; set; }
    #endregion
}