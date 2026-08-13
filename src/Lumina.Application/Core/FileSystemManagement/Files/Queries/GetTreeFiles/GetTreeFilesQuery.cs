#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Query for retrieving the list of files at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of files.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden files and directories or not.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetTreeFilesQuery(
    string? Path, 
    bool IncludeHiddenElements
) : IQuery;
