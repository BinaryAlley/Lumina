#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a literal text segment of a theme template.
/// </summary>
/// <param name="Value">The literal text to render as-is.</param>
[DebuggerDisplay("Value: {Value}")]
public sealed record ThemeTextNodeDto(
    string Value
) : ThemeTemplateNodeDto;
