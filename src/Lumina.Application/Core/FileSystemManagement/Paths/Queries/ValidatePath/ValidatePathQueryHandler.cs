#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Handler for the query to validate a file system path.
/// </summary>
public class ValidatePathQueryHandler : IRequestHandler<ValidatePathQuery, PathValidResponse>
{
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public ValidatePathQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Validates the specified file system path.
    /// </summary>
    /// <param name="request">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// <see langword="true"/> if the specified path is valid, <see langword="false"/> otherwise.
    /// </returns>
    public ValueTask<PathValidResponse> Handle(ValidatePathQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new PathValidResponse(_pathService.IsValidPath(request.Path!)));
    }
}
