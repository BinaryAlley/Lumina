#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to restore a soft deleted bundled theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to restore. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record RestoreThemeRequest(
    string? ThemeId
);
