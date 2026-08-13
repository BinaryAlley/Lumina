#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CombinePath;

/// <summary>
/// API endpoint for the <c>/path/combine</c> route.
/// </summary>
public class CombinePathEndpoint : BaseEndpoint<CombinePathRequest, IResult>
{
    private readonly ICommandHandler<CombinePathCommand, ErrorOr<PathSegmentResponse>> _combinePathCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathEndpoint"/> class.
    /// </summary>
    /// <param name="combinePathCommandHandler">Injected service for handling combine path commands.</param>
    public CombinePathEndpoint(ICommandHandler<CombinePathCommand, ErrorOr<PathSegmentResponse>> combinePathCommandHandler)
    {
        _combinePathCommandHandler = combinePathCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Path.COMBINE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Combines the paths stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the paths to be combined.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CombinePathRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<PathSegmentResponse> result = await _combinePathCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
