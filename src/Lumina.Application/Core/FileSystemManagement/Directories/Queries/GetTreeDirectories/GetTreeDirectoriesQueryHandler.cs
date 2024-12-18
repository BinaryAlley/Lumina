#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetTreeDirectories;

/// <summary>
/// Handler for the query to get all directories.
/// </summary>
public class GetTreeDirectoriesQueryHandler : IRequestHandler<GetTreeDirectoriesQuery, ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>
{
    private readonly IDirectoryService _directoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesQueryHandler"/> class.
    /// </summary>
    /// <param name="directoryService">Injected service for handling directories.</param>
    public GetTreeDirectoriesQueryHandler(IDirectoryService directoryService)
    {
        _directoryService = directoryService;
    }

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
        ErrorOr<IEnumerable<Directory>> getSubdirectoriesResult = _directoryService.GetSubdirectories(request.Path!, request.IncludeHiddenElements);
        return ValueTask.FromResult(getSubdirectoriesResult.Match(values => ErrorOrFactory.From(values.ToFileSystemTreeNodeResponses()), errors => errors));
    }
}
