#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;

/// <summary>
/// Handler for the query to get all files.
/// </summary>
public class GetFilesQueryHandler : IQueryHandler<GetFilesQuery, ErrorOr<IEnumerable<FileResponse>>>
{
    private readonly IFileService _fileService;
    private readonly IValidator<GetFilesQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryHandler"/> class.
    /// </summary>
    /// <param name="fileService">Injected service for handling files.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetFilesQueryHandler(IFileService fileService, IValidator<GetFilesQuery> validator)
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
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="FileResponse"/>, or an error message.
    /// </returns>
    public Task<ErrorOr<IEnumerable<FileResponse>>> HandleAsync(GetFilesQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<IEnumerable<FileResponse>>>(validationResult);

        ErrorOr<IEnumerable<File>> getFilesResult = _fileService.GetFiles(query.Path!, query.IncludeHiddenElements);
        return Task.FromResult(getFilesResult.Match(values => ErrorOrFactory.From(values.ToResponses()), errors => errors));
    }
}
