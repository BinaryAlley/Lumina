#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the template of a theme returned by the remote API.
/// </summary>
/// <param name="Theme">The theme the template belongs to.</param>
/// <param name="Template">The sanitized content of the template.</param>
[DebuggerDisplay("ThemeId: {Theme.ThemeId}")]
public sealed record ThemeTemplateResponseDto(
    ThemeResponseDto Theme,
    string Template
);
