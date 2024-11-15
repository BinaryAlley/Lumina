#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// API endpoint for the <c>/path/get-path-separator</c> route.
/// </summary>
public class GetPathSeparatorEndpoint : EndpointWithoutRequest<IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetPathSeparatorEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Path.GET_PATH_SEPARATOR);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the file system path separator character of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        PathSeparatorResponse result = await _sender.Send(new GetPathSeparatorQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(result);
    }
}
