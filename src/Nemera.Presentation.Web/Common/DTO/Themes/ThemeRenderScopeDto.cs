#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model resolution context used while rendering a theme template.
/// </summary>
/// <param name="Value">The model value this scope resolves expressions against.</param>
/// <param name="Parent">The parent scope, or <see langword="null"/> for the root scope.</param>
[DebuggerDisplay("Value: {Value}")]
public sealed record ThemeRenderScopeDto(
    object? Value,
    ThemeRenderScopeDto? Parent
);
