#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Handler for the query to validate a file system path.
/// </summary>
public class ValidatePathQueryHandler : IRequestHandler<ValidatePathQuery, PathValidResponse>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IPathService _pathService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    public ValidatePathQueryHandler(IPathService pathService)
    {
        _pathService = pathService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
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
        return ValueTask.FromResult(new PathValidResponse(_pathService.IsValidPath(request.Path)));
    }
    #endregion
}