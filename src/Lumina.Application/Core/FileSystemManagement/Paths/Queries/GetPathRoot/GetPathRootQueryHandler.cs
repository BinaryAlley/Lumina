#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Handler for the query to get the root of a file system path.
/// </summary>
public class GetPathRootQueryHandler : IQueryHandler<GetPathRootQuery, ErrorOr<PathSegmentResponse>>
{
    private readonly IPathService _pathService;
    private readonly IValidator<GetPathRootQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetPathRootQueryHandler(IPathService pathService, IValidator<GetPathRootQuery> validator)
    {
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Gets the root of the specified file system path.
    /// </summary>
    /// <param name="query">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the root of the specified path, or an error.</returns>
    public Task<ErrorOr<PathSegmentResponse>> HandleAsync(GetPathRootQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<PathSegmentResponse>>(validationResult);

        ErrorOr<PathSegment> getPathRootResult = _pathService.GetPathRoot(query.Path!);
        return Task.FromResult(getPathRootResult.Match(values => ErrorOrFactory.From(values.ToResponse()), errors => errors));
    }
}
