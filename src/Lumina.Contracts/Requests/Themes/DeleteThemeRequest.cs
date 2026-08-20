#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to delete a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to delete. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record DeleteThemeRequest(
    string? ThemeId
);
