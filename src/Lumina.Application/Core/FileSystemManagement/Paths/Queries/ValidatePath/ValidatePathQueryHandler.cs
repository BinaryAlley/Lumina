#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Handler for the query to validate a file system path.
/// </summary>
public class ValidatePathQueryHandler : IQueryHandler<ValidatePathQuery, Result<PathValidResponse>>
{
    private readonly IPathService _pathService;
    private readonly IValidator<ValidatePathQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public ValidatePathQueryHandler(IPathService pathService, IValidator<ValidatePathQuery> validator)
    {
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Validates the specified file system path.
    /// </summary>
    /// <param name="query">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either <see langword="true"/> if the specified path is valid, <see langword="false"/> if it isn't, or an error message.
    /// </returns>
    public Task<Result<PathValidResponse>> HandleAsync(ValidatePathQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<Result<PathValidResponse>>(validationResult);

        return Task.FromResult(Result.From(new PathValidResponse(_pathService.IsValidPath(query.Path!))));
    }
}
