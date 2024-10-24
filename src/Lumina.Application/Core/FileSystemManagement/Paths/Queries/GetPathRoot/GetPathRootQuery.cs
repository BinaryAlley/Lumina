#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Mediator;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Query for retrieving the root of a path.
/// </summary>
/// <param name="Path">The path for which to get the path root.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetPathRootQuery(
    string? Path
) : IRequest<ErrorOr<PathSegmentResponse>>;
