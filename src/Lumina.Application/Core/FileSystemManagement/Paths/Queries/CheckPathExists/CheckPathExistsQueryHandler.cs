#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Handler for the query to check the existence a file system path.
/// </summary>
public class CheckPathExistsQueryHandler : IRequestHandler<CheckPathExistsQuery, ErrorOr<PathExistsResponse>>
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
    /// An <see cref="ErrorOr{TValue}"/> containing either <see langword="true"/> if the specified path exists, <see langword="false"/> if it doesn't, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<PathExistsResponse>> Handle(CheckPathExistsQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(ErrorOrFactory.From(new PathExistsResponse(_pathService.Exists(request.Path!, request.IncludeHiddenElements))));
    }
}
