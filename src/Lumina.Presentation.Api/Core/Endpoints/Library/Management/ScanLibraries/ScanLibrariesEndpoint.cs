#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// API endpoint for the <c>/libraries/scan</c> route.
/// </summary>
public class ScanLibrariesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public ScanLibrariesEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiRoutes.Libraries.SCAN_LIBRARIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Initiates a scan of all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await _sender.Send(new ScanLibrariesCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
