#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.ScanLibrary;

/// <summary>
/// API endpoint for the <c>/libraries/{id}/scans</c> route.
/// </summary>
public class ScanLibraryEndpoint : BaseEndpoint<ScanLibraryRequest, IResult>
{
    private readonly ICommandHandler<ScanLibraryCommand, Result<MediaLibraryScanResponse>> _scanLibraryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="scanLibraryCommandHandler">Injected service for handling scan library commands.</param>
    public ScanLibraryEndpoint(ICommandHandler<ScanLibraryCommand, Result<MediaLibraryScanResponse>> scanLibraryCommandHandler)
    {
        _scanLibraryCommandHandler = scanLibraryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Libraries.SCAN_LIBRARY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Initiates a scan of a media library.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ScanLibraryRequest request, CancellationToken cancellationToken)
    {
        Result<MediaLibraryScanResponse> result = await _scanLibraryCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
