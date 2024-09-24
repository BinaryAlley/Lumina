#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Query for retrieving the list of files at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of files.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden files and directories or not.</param>
public record GetTreeFilesQuery(string Path, bool IncludeHiddenElements) : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;