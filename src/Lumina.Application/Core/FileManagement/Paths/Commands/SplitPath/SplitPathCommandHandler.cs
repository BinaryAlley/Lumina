#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using MapsterMapper;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Commands.SplitPath;

/// <summary>
/// Handler for the command to split a file system path.
/// </summary>
public class SplitPathCommandHandler : IRequestHandler<SplitPathCommand, ErrorOr<IEnumerable<PathSegmentResponse>>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IPathService _pathService;
    private readonly IMapper _mapper;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="mapper">Injected service for mapping objects.</param>
    public SplitPathCommandHandler(IPathService pathService, IMapper mapper)
    {
        _pathService = pathService;
        _mapper = mapper;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
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
        ErrorOr<IEnumerable<PathSegment>> result = _pathService.ParsePath(request.Path);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(_mapper.Map<IEnumerable<PathSegmentResponse>>(values)), errors => errors));
    }
    #endregion
}
