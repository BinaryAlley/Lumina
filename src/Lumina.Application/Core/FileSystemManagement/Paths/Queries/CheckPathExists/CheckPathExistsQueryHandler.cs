#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Handler for the query to check the existence a file system path.
/// </summary>
public class CheckPathExistsQueryHandler : IQueryHandler<CheckPathExistsQuery, ErrorOr<PathExistsResponse>>
{
    private readonly IPathService _pathService;
    private readonly IValidator<CheckPathExistsQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public CheckPathExistsQueryHandler(IPathService pathService, IValidator<CheckPathExistsQuery> validator)
    {
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Checks the existence of the specified file system path.
    /// </summary>
    /// <param name="query">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either <see langword="true"/> if the specified path exists, <see langword="false"/> if it doesn't, or an error message.
    /// </returns>
    public Task<ErrorOr<PathExistsResponse>> HandleAsync(CheckPathExistsQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<PathExistsResponse>>(validationResult);

        return Task.FromResult(ErrorOrFactory.From(new PathExistsResponse(_pathService.Exists(query.Path!, query.IncludeHiddenElements))));
    }
}
