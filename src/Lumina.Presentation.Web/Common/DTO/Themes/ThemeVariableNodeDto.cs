#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a variable expression in a theme template.
/// </summary>
/// <param name="Expression">The property path expression to resolve.</param>
/// <param name="ShouldBeEscaped">Whether the rendered value should be HTML encoded.</param>
[DebuggerDisplay("Expression: {Expression}")]
public sealed record ThemeVariableNodeDto(
    string Expression,
    bool ShouldBeEscaped
) : ThemeTemplateNodeDto;
