#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries;

/// <summary>
/// Handler for the query to get the path separator character of a file system path.
/// </summary>
public class GetPathSeparatorQueryHandler : IRequestHandler<GetPathSeparatorQuery, PathSeparatorResponse>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IPathService _pathService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public GetPathSeparatorQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the path separator character of a file system path.
    /// </summary>
    /// <param name="request">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The file system path separator character.</returns>
    public ValueTask<PathSeparatorResponse> Handle(GetPathSeparatorQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new PathSeparatorResponse(_pathService.PathSeparator.ToString()));
    }
    #endregion
}