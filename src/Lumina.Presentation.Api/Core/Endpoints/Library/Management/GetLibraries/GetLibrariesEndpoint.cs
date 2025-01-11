#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraries;

/// <summary>
/// API endpoint for the <c>/libraries</c> route.
/// </summary>
public class GetLibrariesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetLibrariesEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of libraries.
    /// </summary>
    /// <param name="request">The request containing the id of the library to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<LibraryResponse[]> result = await _sender.Send(new GetLibrariesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
