#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Query for retrieving the list of directories at a path.
/// </summary>
/// <param name="Path">The path for which to retrieve the list of directories.</param>
public record GetDirectoriesQuery(string Path) : IRequest<ErrorOr<IEnumerable<DirectoryResponse>>>;