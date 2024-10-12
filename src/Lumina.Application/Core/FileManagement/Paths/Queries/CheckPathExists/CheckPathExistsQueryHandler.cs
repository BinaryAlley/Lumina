#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Handler for the query to check the existence a file system path.
/// </summary>
public class CheckPathExistsQueryHandler : IRequestHandler<CheckPathExistsQuery, PathExistsResponse>
{
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public CheckPathExistsQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Checks the existence of the specified file system path.
    /// </summary>
    /// <param name="request">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// <see langword="true"/> if the specified path exists, <see langword="false"/> otherwise.
    /// </returns>
    public ValueTask<PathExistsResponse> Handle(CheckPathExistsQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new PathExistsResponse(_pathService.Exists(request.Path, request.IncludeHiddenElements)));
    }
}
