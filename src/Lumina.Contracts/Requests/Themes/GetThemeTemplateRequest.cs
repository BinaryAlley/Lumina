#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to get the template of a theme selected by a page key.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme. Required.</param>
/// <param name="PageKey">The page key that selects the template. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}, PageKey: {PageKey}")]
public record GetThemeTemplateRequest(
    string? ThemeId,
    string? PageKey
);
