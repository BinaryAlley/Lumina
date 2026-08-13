#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// API endpoint for the <c>/libraries/scans</c> route.
/// </summary>
public class ScanLibrariesEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<ScanLibrariesCommand, ErrorOr<IEnumerable<MediaLibraryScanResponse>>> _scanLibrariesCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpoint"/> class.
    /// </summary>
    /// <param name="scanLibrariesCommandHandler">Injected service for handling scan libraries commands.</param>
    public ScanLibrariesEndpoint(ICommandHandler<ScanLibrariesCommand, ErrorOr<IEnumerable<MediaLibraryScanResponse>>> scanLibrariesCommandHandler)
    {
        _scanLibrariesCommandHandler = scanLibrariesCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Libraries.SCAN_LIBRARIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Initiates a scan of all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<MediaLibraryScanResponse>> result = await _scanLibrariesCommandHandler.HandleAsync(new ScanLibrariesCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
