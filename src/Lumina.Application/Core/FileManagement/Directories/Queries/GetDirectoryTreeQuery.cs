#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries;

/// <summary>
/// Query for retrieving the tree of directories of a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the tree of directories.</param>
/// <param name="IncludeFiles">Whether to include files in the directory tree or not.</param>
public record GetDirectoryTreeQuery(string Path, bool IncludeFiles) : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;