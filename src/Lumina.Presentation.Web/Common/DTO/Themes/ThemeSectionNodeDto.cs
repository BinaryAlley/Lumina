#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a conditional or repeating section in a theme template.
/// </summary>
/// <param name="Expression">The property path expression that controls the section.</param>
/// <param name="Inverted">Whether the section renders when the resolved value is falsy instead of truthy.</param>
/// <param name="Children">The template nodes rendered when the section is active.</param>
[DebuggerDisplay("Expression: {Expression}")]
public sealed record ThemeSectionNodeDto(
    string Expression,
    bool Inverted,
    IReadOnlyList<ThemeTemplateNodeDto> Children
) : ThemeTemplateNodeDto;
