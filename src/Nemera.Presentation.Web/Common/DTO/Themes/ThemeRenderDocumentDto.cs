#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a rendered document.
/// </summary>
/// <param name="Theme">The metadata of the resolved theme.</param>
/// <param name="Template">The raw template source to render.</param>
[DebuggerDisplay("Theme name: {Theme.Name}")]
public sealed record ThemeRenderDocumentDto(
    ThemeInfoDto Theme,
    string Template
);
