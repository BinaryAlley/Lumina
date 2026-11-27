#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;

/// <summary>
/// Command for restoring a soft deleted bundled theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to restore.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record RestoreThemeCommand(
    string? ThemeId
) : ICommand;
