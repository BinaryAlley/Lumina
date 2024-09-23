#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Mapster;
using MapsterMapper;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;

/// <summary>
/// Handler for the query to get all directories.
/// </summary>
public class GetTreeDirectoriesQueryHandler : IRequestHandler<GetTreeDirectoriesQuery, ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IDirectoryService _directoryService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesQueryHandler"/> class.
    /// </summary>
    /// <param name="directoryService">Injected service for handling directories.</param>
    public GetTreeDirectoriesQueryHandler(IDirectoryService directoryService)
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
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>> Handle(GetTreeDirectoriesQuery request, CancellationToken cancellationToken)
    {
        var result = _directoryService.GetSubdirectories(request.Path);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(result.Value.Adapt<IEnumerable<FileSystemTreeNodeResponse>>()), errors => errors));
    }
    #endregion
}