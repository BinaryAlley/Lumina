#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;

/// <summary>
/// Command for deleting a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to delete.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record DeleteThemeCommand(
    string? ThemeId
) : ICommand;
