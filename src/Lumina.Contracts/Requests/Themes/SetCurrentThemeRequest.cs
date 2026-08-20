#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to set the currently active theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to activate. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record SetCurrentThemeRequest(
    string? ThemeId
);
