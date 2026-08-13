#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Handler for the query to get all directories.
/// </summary>
public class GetDirectoriesQueryHandler : IQueryHandler<GetDirectoriesQuery, ErrorOr<IEnumerable<DirectoryResponse>>>
{
    private readonly IDirectoryService _directoryService;
    private readonly IValidator<GetDirectoriesQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryHandler"/> class.
    /// </summary>
    /// <param name="directoryService">Injected service for handling directories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetDirectoriesQueryHandler(IDirectoryService directoryService, IValidator<GetDirectoriesQuery> validator)
    {
        _directoryService = directoryService;
        _validator = validator;
    }

    /// <summary>
    /// Gets the list of directories at the specified path.
    /// </summary>
    /// <param name="query">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="DirectoryResponse"/>, or an error message.
    /// </returns>
    public Task<ErrorOr<IEnumerable<DirectoryResponse>>> HandleAsync(GetDirectoriesQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<IEnumerable<DirectoryResponse>>>(validationResult);

        ErrorOr<IEnumerable<Directory>> getSubdirectoriesResult = _directoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements);
        return Task.FromResult(getSubdirectoriesResult.Match(values => ErrorOrFactory.From(values.ToResponses()), errors => errors));
    }
}
