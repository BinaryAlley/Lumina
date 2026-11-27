#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Query for retrieving the root of a path.
/// </summary>
/// <param name="Path">The path for which to get the path root.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetPathRootQuery(
    string? Path
) : IQuery;
