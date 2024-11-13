#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Mediator;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Query for checking the existence of a file system path.
/// </summary>
/// <param name="Path">The path for which to check the existence of.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden file system elements or not.</param>
[DebuggerDisplay("Path: {Path}, IncludeHiddenElements: {IncludeHiddenElements}")]
public record CheckPathExistsQuery(
    string? Path, 
    bool IncludeHiddenElements
) : IRequest<PathExistsResponse>;
