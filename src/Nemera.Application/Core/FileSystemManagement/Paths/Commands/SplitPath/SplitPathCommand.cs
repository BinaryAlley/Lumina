#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Command for splitting a file system path.
/// </summary>
/// <param name="Path">The file system path to split.</param>
[DebuggerDisplay("Path: {Path}")]
public record SplitPathCommand(
    string? Path
) : ICommand;
