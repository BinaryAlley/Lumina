#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Handler for the command to split a file system path.
/// </summary>
public class SplitPathCommandHandler : IRequestHandler<SplitPathCommand, ErrorOr<IEnumerable<PathSegmentResponse>>>
{
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    public SplitPathCommandHandler(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Gets the path components of the specified path.
    /// </summary>
    /// <param name="request">The command containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PathSegmentResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<IEnumerable<PathSegmentResponse>>> Handle(SplitPathCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<PathSegment>> result = _pathService.ParsePath(request.Path!);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(values.ToResponses()), errors => errors));
    }
}
