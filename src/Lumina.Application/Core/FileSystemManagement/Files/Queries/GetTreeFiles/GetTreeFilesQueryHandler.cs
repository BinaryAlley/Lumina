#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Handler for the query to get all files.
/// </summary>
public class GetTreeFilesQueryHandler : IQueryHandler<GetTreeFilesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>>
{
    private readonly IFileService _fileService;
    private readonly IValidator<GetTreeFilesQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryHandler"/> class.
    /// </summary>
    /// <param name="fileService">Injected service for handling files.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetTreeFilesQueryHandler(IFileService fileService, IValidator<GetTreeFilesQuery> validator)
    {
        _fileService = fileService;
        _validator = validator;
    }

    /// <summary>
    /// Gets the list of files at the specified path.
    /// </summary>
    /// <param name="query">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    public Task<Result<IEnumerable<FileSystemTreeNodeResponse>>> HandleAsync(GetTreeFilesQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<Result<IEnumerable<FileSystemTreeNodeResponse>>>(validationResult);

        Result<IEnumerable<File>> getFilesResult = _fileService.GetFiles(query.Path!, query.IncludeHiddenElements);
        return Task.FromResult(getFilesResult.Match(values => Result.From(values.ToFileSystemTreeNodeResponses()), errors => errors));
    }
}
