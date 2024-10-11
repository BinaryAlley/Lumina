#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Query for retrieving the existence of a file system path.
/// </summary>
/// <param name="Path">The path for which to check the existence.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden file system elements or not.</param>
public record CheckPathExistsQuery(string Path, bool IncludeHiddenElements) : IRequest<PathExistsResponse>;
