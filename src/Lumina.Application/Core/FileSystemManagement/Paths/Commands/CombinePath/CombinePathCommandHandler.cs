#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Handler for the command to split a file system path.
/// </summary>
public class CombinePathCommandHandler : IRequestHandler<CombinePathCommand, ErrorOr<PathSegmentResponse>>
{
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    public CombinePathCommandHandler(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Combines two file system paths.
    /// </summary>
    /// <param name="request">The command containing the requested paths.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="PathSegmentResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<PathSegmentResponse>> Handle(CombinePathCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<string> result = _pathService.CombinePath(request.OriginalPath!, request.NewPath!);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(new PathSegmentResponse(result.Value)), errors => errors));
    }
}
