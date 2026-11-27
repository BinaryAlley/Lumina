#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.FileSystemManagement.Paths;
using Nemera.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Nemera.Contracts.Requests.FileSystemManagement.Path;
using Nemera.Contracts.Responses.FileSystemManagement.Path;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.FileSystemManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CombinePath;

/// <summary>
/// API endpoint for the <c>/path/combine</c> route.
/// </summary>
public class CombinePathEndpoint : BaseEndpoint<CombinePathRequest, IResult>
{
    private readonly ICommandHandler<CombinePathCommand, Result<PathSegmentResponse>> _combinePathCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathEndpoint"/> class.
    /// </summary>
    /// <param name="combinePathCommandHandler">Injected service for handling combine path commands.</param>
    public CombinePathEndpoint(ICommandHandler<CombinePathCommand, Result<PathSegmentResponse>> combinePathCommandHandler)
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
        Result<PathSegmentResponse> result = await _combinePathCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
