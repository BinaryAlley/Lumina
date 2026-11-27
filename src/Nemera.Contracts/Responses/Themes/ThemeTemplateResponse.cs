#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Themes;

/// <summary>
/// Represents a theme template response.
/// </summary>
/// <param name="Theme">The theme the template belongs to.</param>
/// <param name="Template">The sanitized content of the template.</param>
[DebuggerDisplay("ThemeId: {Theme.ThemeId}")]
public record ThemeTemplateResponse(
    ThemeResponse Theme,
    string Template
);
