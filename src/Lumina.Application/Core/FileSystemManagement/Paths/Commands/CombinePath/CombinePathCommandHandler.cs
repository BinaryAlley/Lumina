#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Handler for the command to split a file system path.
/// </summary>
public class CombinePathCommandHandler : ICommandHandler<CombinePathCommand, ErrorOr<PathSegmentResponse>>
{
    private readonly IPathService _pathService;
    private readonly IValidator<CombinePathCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandler"/> class.
    /// </summary>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public CombinePathCommandHandler(IPathService pathService, IValidator<CombinePathCommand> validator)
    {
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Combines two file system paths.
    /// </summary>
    /// <param name="command">The command containing the requested paths.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="PathSegmentResponse"/>, or an error message.
    /// </returns>
    public Task<ErrorOr<PathSegmentResponse>> HandleAsync(CombinePathCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return Task.FromResult<ErrorOr<PathSegmentResponse>>(validationResult);

        ErrorOr<string> combinePathResult = _pathService.CombinePath(command.OriginalPath!, command.NewPath!);
        return Task.FromResult(combinePathResult.Match(values => ErrorOrFactory.From(new PathSegmentResponse(combinePathResult.Value)), errors => errors));
    }
}
