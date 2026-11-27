#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.InstallPlugin;

/// <summary>
/// Command for installing a plugin from an uploaded archive, placing its assemblies into the plugin storage directory.
/// </summary>
/// <param name="Archive">The archive stream of the uploaded plugin.</param>
/// <param name="FileName">The file name of the uploaded plugin.</param>
[DebuggerDisplay("FileName: {FileName}")]
public record InstallPluginCommand(
    Stream? Archive,
    string? FileName
) : ICommand;
