#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Mediator;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;

/// <summary>
/// Query for retrieving the parent of a file system path.
/// </summary>
/// <param name="Path">The path for which to get the parent.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetPathParentQuery(
    string? Path
) : IRequest<ErrorOr<IEnumerable<PathSegmentResponse>>>;
