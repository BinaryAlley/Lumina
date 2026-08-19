#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the settings persisted for the theme engine.
/// </summary>
[DebuggerDisplay("CurrentThemeId: {CurrentThemeId}")]
public sealed class PersistedThemeSettingsDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the currently selected theme.
    /// </summary>
    public string CurrentThemeId { get; set; } = string.Empty;
}
