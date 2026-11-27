#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Query for retrieving the list of directories at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of directories.</param>
/// <param name="ShouldIncludeHiddenElements">Whether to include hidden directories or not.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetDirectoriesQuery(
    string? Path, 
    bool ShouldIncludeHiddenElements
) : IQuery;
