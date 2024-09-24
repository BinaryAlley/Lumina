#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;

/// <summary>
/// Query for retrieving the tree of directories of a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the tree of directories.</param>
/// <param name="IncludeFiles">Whether to include files in the directory tree or not.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden files and directories or not.</param>
public record GetDirectoryTreeQuery(string Path, bool IncludeFiles, bool IncludeHiddenElements) : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;
