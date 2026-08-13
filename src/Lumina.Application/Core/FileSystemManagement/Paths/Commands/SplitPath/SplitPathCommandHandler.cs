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

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Handler for the command to split a file system path.
/// </summary>
public class SplitPathCommandHandler : ICommandHandler<SplitPathCommand, ErrorOr<IEnumerable<PathSegmentResponse>>>
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
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PathSegmentResponse"/>, or an error message.
    /// </returns>
    public Task<ErrorOr<IEnumerable<PathSegmentResponse>>> HandleAsync(SplitPathCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<IEnumerable<PathSegmentResponse>>>(validationResult);

        ErrorOr<IEnumerable<PathSegment>> parsePathResult = _pathService.ParsePath(command.Path!);
        return Task.FromResult(parsePathResult.Match(values => ErrorOrFactory.From(values.ToResponses()), errors => errors));
    }
}
