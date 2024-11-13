#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;

/// <summary>
/// Query for retrieving the file system path separator.
/// </summary>
public record GetPathSeparatorQuery() : IRequest<PathSeparatorResponse>;