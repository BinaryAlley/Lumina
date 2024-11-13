#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Handler for the query to get all files.
/// </summary>
public class GetTreeFilesQueryHandler : IRequestHandler<GetTreeFilesQuery, ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>
{
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryHandler"/> class.
    /// </summary>
    /// <param name="fileService">Injected service for handling files.</param>
    public GetTreeFilesQueryHandler(IFileService fileService)
    {
        _fileService = fileService;
    }

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
        ErrorOr<IEnumerable<File>> result = _fileService.GetFiles(request.Path!, request.IncludeHiddenElements);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(values.ToFileSystemTreeNodeResponses()), errors => errors));
    }
}
