#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Mapster;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Handler for the query to get all directories.
/// </summary>
public class GetDirectoriesQueryHandler : IRequestHandler<GetDirectoriesQuery, ErrorOr<IEnumerable<DirectoryResponse>>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IDirectoryService _directoryService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryHandler"/> class.
    /// </summary>
    /// <param name="directoryService">Injected service for handling directories.</param>
    public GetDirectoriesQueryHandler(IDirectoryService directoryService)
    {
        _directoryService = directoryService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the list of directories at the specified path.
    /// </summary>
    /// <param name="request">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="DirectoryResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<IEnumerable<DirectoryResponse>>> Handle(GetDirectoriesQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<Directory>> result = _directoryService.GetSubdirectories(request.Path, request.IncludeHiddenElements);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(result.Value.Adapt<IEnumerable<DirectoryResponse>>()), errors => errors));
    }
    #endregion
}