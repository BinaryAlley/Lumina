#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Mediator;
using System.Collections.Generic;
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
) : IRequest<ErrorOr<IEnumerable<PathSegmentResponse>>>;
