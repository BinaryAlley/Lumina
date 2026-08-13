#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/scans/{scanId}/cancel</c> route.
/// </summary>
public class CancelLibraryScanEndpoint : BaseEndpoint<CancelLibraryScanRequest, IResult>
{
    private readonly ICommandHandler<CancelLibraryScanCommand, ErrorOr<Success>> _cancelLibraryScanCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpoint"/> class.
    /// </summary>
    /// <param name="cancelLibraryScanCommandHandler">Injected service for handling cancel library scan commands.</param>
    public CancelLibraryScanEndpoint(ICommandHandler<CancelLibraryScanCommand, ErrorOr<Success>> cancelLibraryScanCommandHandler)
    {
        _cancelLibraryScanCommandHandler = cancelLibraryScanCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Libraries.CANCEL_LIBRARY_SCAN);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Cancels a previously started scan of all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CancelLibraryScanRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await _cancelLibraryScanCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
