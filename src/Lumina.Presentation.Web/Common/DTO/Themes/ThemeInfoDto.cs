#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the display metadata of a theme.
/// </summary>
/// <param name="Id">The unique identifier of the theme.</param>
/// <param name="Name">The display name of the theme.</param>
/// <param name="Description">The description of the theme.</param>
/// <param name="Author">The author of the theme.</param>
/// <param name="Version">The version of the theme, using semantic version form.</param>
/// <param name="PreviewUrl">The URL of the theme preview image.</param>
/// <param name="IsBundled">Whether the theme ships with the application or noy.</param>
/// <param name="IsDeleted">Whether the theme was deleted by the user, which is only possible for bundled themes.</param>
[DebuggerDisplay("Id: {Id}, Name: {Name}")]
public sealed record ThemeInfoDto(
    string Id,
    string Name,
    string Description,
    string Author,
    string Version,
    string PreviewUrl,
    bool IsBundled,
    bool IsDeleted
);
