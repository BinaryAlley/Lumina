#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to download the archive of a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record GetThemeArchiveRequest(
    string? ThemeId
);
