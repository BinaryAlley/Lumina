#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;

/// <summary>
/// Handler for the query to get the path separator character of a file system path.
/// </summary>
public class GetPathSeparatorQueryHandler : IQueryHandler<GetPathSeparatorQuery, PathSeparatorResponse>
{
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public GetPathSeparatorQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Gets the path separator character of a file system path.
    /// </summary>
    /// <param name="query">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The file system path separator character.</returns>
    public Task<PathSeparatorResponse> HandleAsync(GetPathSeparatorQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PathSeparatorResponse(_pathService.PathSeparator.ToString()));
    }
}