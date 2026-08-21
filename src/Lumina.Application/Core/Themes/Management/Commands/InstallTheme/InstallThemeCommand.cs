#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.InstallTheme;

/// <summary>
/// Command for installing a theme pack, replacing the files of an existing theme with the same manifest id.
/// </summary>
/// <param name="Archive">The ZIP archive stream of the theme pack.</param>
/// <param name="FileName">The file name of the uploaded archive.</param>
[DebuggerDisplay("FileName: {FileName}")]
public record InstallThemeCommand(
    Stream? Archive,
    string? FileName
) : ICommand;
