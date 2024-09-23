#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Query for retrieving the existence of a file system path.
/// </summary>
/// <param name="Path">The path for which to check the existence.</param>
public record CheckPathExistsQuery(string Path) : IRequest<PathExistsResponse>;