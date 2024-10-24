#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Handler for the query to get the root of a file system path.
/// </summary>
public class GetPathRootQueryHandler : IRequestHandler<GetPathRootQuery, ErrorOr<PathSegmentResponse>>
{
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public GetPathRootQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Gets the root of the specified file system path.
    /// </summary>
    /// <param name="request">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The root of the specified path.</returns>
    public ValueTask<ErrorOr<PathSegmentResponse>> Handle(GetPathRootQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<PathSegment> result = _pathService.GetPathRoot(request.Path!);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(values.ToResponse()), errors => errors));
    }
}
