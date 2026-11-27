#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Command for splitting a file system path.
/// </summary>
/// <param name="Path">The file system path to split.</param>
[DebuggerDisplay("Path: {Path}")]
public record SplitPathCommand(
    string? Path
) : ICommand;
