#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.UsersManagement;

/// <summary>
/// Data transfer object for the settings of a user.
/// </summary>
[DebuggerDisplay("IsPaginationEnabled: {IsPaginationEnabled}; ItemsPerPage: {ItemsPerPage}")]
public class UserSettingsDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user that owns these settings.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets whether pagination is enabled for the user, or not.
    /// </summary>
    public bool IsPaginationEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of library items displayed per page when pagination is enabled.
    /// </summary>
    public int ItemsPerPage { get; set; } = 48;

    /// <summary>
    /// Gets or sets whether the "The" prefix of library item titles is ignored by the alpha picker, or not.
    /// </summary>
    public bool ShouldIgnoreThePrefixForAlphaPicker { get; set; }

    /// <summary>
    /// Gets or sets whether the theme data served to this user is cached, or not.
    /// </summary>
    public bool IsThemeCachingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.
    /// </summary>
    public bool ShouldAggregateMetadataWhenMissing { get; set; }
}
