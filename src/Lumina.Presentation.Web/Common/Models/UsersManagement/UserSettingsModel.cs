#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Represents the settings of a user.
/// </summary>
[DebuggerDisplay("IsPaginationEnabled: {IsPaginationEnabled}; ItemsPerPage: {ItemsPerPage}")]
public class UserSettingsModel
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
    public bool IgnoreThePrefixForAlphaPicker { get; set; }
}
