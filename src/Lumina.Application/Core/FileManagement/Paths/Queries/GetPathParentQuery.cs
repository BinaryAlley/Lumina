#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries;

/// <summary>
/// Query for retrieving the parent of a file system path.
/// </summary>
/// <param name="Path">The path for which to get the parent.</param>
public record GetPathParentQuery(string Path) : IRequest<ErrorOr<IEnumerable<PathSegmentResponse>>>;