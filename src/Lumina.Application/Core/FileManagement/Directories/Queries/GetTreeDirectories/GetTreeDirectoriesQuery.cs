#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;

/// <summary>
/// Query for retrieving the list of directories at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of directories.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden directories or not.</param>
public record GetTreeDirectoriesQuery(string Path, bool IncludeHiddenElements) : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;