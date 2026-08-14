#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.SplitPath;

/// <summary>
/// API endpoint for the <c>/path/split</c> route.
/// </summary>
public class SplitPathEndpoint : BaseEndpoint<SplitPathRequest, IResult>
{
    private readonly ICommandHandler<SplitPathCommand, Result<IEnumerable<PathSegmentResponse>>> _splitPathCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathEndpoint"/> class.
    /// </summary>
    /// <param name="splitPathCommandHandler">Injected service for handling split path commands.</param>
    public SplitPathEndpoint(ICommandHandler<SplitPathCommand, Result<IEnumerable<PathSegmentResponse>>> splitPathCommandHandler)
    {
        _splitPathCommandHandler = splitPathCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Path.SPLIT);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the path components of a file system path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SplitPathRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<PathSegmentResponse>> result = await _splitPathCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
