namespace Lumina.Presentation.Web.Common.Models.Contributors;

/// <summary>
/// Represents a media contributor.
/// </summary>
public class MediaContributorDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the contributor.
    /// </summary>
    public MediaContributorNameDto? Name { get; set; }

    /// <summary>
    /// Gets the role of the contributor.
    /// </summary>
    public MediaContributorRoleDto? Role { get; set; }
    #endregion
}