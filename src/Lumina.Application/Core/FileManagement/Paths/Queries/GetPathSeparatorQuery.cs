#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries;

/// <summary>
/// Query for retrieving the file system path separator.
/// </summary>
public record GetPathSeparatorQuery() : IRequest<PathSeparatorResponse>;