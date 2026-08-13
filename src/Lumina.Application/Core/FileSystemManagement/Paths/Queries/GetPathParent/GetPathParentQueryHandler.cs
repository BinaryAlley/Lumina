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

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;

/// <summary>
/// Handler for the query to get the parent of a file system path.
/// </summary>
public class GetPathParentQueryHandler : IQueryHandler<GetPathParentQuery, ErrorOr<IEnumerable<PathSegmentResponse>>>
{
    private readonly IPathService _pathService;
    private readonly IValidator<GetPathParentQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentQueryHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for managing file system paths.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetPathParentQueryHandler(IPathService pathService, IValidator<GetPathParentQuery> validator)
    {
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Gets the parent of the specified file system path.
    /// </summary>
    /// <param name="query">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PathSegmentResponse"/>, or an error message.
    /// </returns>
    public Task<ErrorOr<IEnumerable<PathSegmentResponse>>> HandleAsync(GetPathParentQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<IEnumerable<PathSegmentResponse>>>(validationResult);

        ErrorOr<IEnumerable<PathSegment>> goUpOneLevelResult = _pathService.GoUpOneLevel(query.Path!);
        return Task.FromResult(goUpOneLevelResult.Match(values => ErrorOrFactory.From(values.ToResponses()), errors => errors));
    }
}
