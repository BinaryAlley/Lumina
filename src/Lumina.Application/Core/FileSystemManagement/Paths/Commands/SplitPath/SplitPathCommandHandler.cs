#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
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

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Handler for the command to split a file system path.
/// </summary>
public class SplitPathCommandHandler : ICommandHandler<SplitPathCommand, Result<IEnumerable<PathSegmentResponse>>>
{
    private readonly IPathService _pathService;
    private readonly IValidator<SplitPathCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public SplitPathCommandHandler(IPathService pathService, IValidator<SplitPathCommand> validator)
    {
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Gets the path components of the specified path.
    /// </summary>
    /// <param name="command">The command containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="PathSegmentResponse"/>, or an error message.
    /// </returns>
    public Task<Result<IEnumerable<PathSegmentResponse>>> HandleAsync(SplitPathCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return Task.FromResult<Result<IEnumerable<PathSegmentResponse>>>(validationResult);

        Result<IEnumerable<PathSegment>> parsePathResult = _pathService.ParsePath(command.Path!);
        return Task.FromResult(parsePathResult.Match(values => Result.From(values.ToResponses()), errors => errors));
    }
}
