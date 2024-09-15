#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Files.Queries;

/// <summary>
/// Query for retrieving the list of files at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of files.</param>
public record GetFilesQuery(string Path) : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;