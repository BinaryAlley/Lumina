#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries;

/// <summary>
/// Query for retrieving the root of a path.
/// </summary>
/// <param name="Path">The path for which to get the path root.</param>
public record GetPathRootQuery(string Path) : IRequest<ErrorOr<PathSegmentResponse>>;