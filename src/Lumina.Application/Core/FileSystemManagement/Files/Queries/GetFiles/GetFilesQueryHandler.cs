#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;

/// <summary>
/// Handler for the query to get all files.
/// </summary>
public class GetFilesQueryHandler : IRequestHandler<GetFilesQuery, ErrorOr<IEnumerable<FileResponse>>>
{
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryHandler"/> class.
    /// </summary>
    /// <param name="fileService">Injected service for handling files.</param>
    public GetFilesQueryHandler(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Gets the list of files at the specified path.
    /// </summary>
    /// <param name="request">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="FileResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<IEnumerable<FileResponse>>> Handle(GetFilesQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<File>> result = _fileService.GetFiles(request.Path!, request.IncludeHiddenElements);
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(values.ToResponses()), errors => errors));
    }
}