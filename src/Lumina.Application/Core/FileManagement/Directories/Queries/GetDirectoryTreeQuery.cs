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
public record GetDirectoryTreeQuery(string Path) : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;