#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;

/// <summary>
/// Command for setting the currently active theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to activate.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record SetCurrentThemeCommand(
    string? ThemeId
) : ICommand;
