#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Handler for the query to get the root of a file system path.
/// </summary>
public class GetPathRootQueryHandler : IRequestHandler<GetPathRootQuery, ErrorOr<PathSegmentResponse>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IPathService _pathService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public GetPathRootQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the root of the specified file system path.
    /// </summary>
    /// <param name="request">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The root of the specified path.</returns>
    public ValueTask<ErrorOr<PathSegmentResponse>> Handle(GetPathRootQuery request, CancellationToken cancellationToken)
    {
        var result = _pathService.GetPathRoot(request.Path);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(result.Value.Adapt<PathSegmentResponse>()), errors => errors));
    }
    #endregion
}