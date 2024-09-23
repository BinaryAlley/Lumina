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

namespace Lumina.Application.Core.FileManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Handler for the query to get all files.
/// </summary>
public class GetTreeFilesQueryHandler : IRequestHandler<GetTreeFilesQuery, ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFileService _fileService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryHandler"/> class.
    /// </summary>
    /// <param name="fileService">Injected service for handling files.</param>
    public GetTreeFilesQueryHandler(IFileService fileService)
    {
        _fileService = fileService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the list of files at the specified path.
    /// </summary>
    /// <param name="request">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>> Handle(GetTreeFilesQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<File>> result = _fileService.GetFiles(request.Path);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(result.Value.Adapt<IEnumerable<FileSystemTreeNodeResponse>>()), errors => errors));
    }
    #endregion
}