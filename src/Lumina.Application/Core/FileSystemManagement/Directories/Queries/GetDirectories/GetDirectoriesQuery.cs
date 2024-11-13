#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Mediator;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Query for retrieving the list of directories at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of directories.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden directories or not.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetDirectoriesQuery(
    string? Path, 
    bool IncludeHiddenElements
) : IRequest<ErrorOr<IEnumerable<DirectoryResponse>>>;
