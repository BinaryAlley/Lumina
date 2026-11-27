#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.Management;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/scans/{scanId}/cancel</c> route.
/// </summary>
public class CancelLibraryScanEndpoint : BaseEndpoint<CancelLibraryScanRequest, IResult>
{
    private readonly ICommandHandler<CancelLibraryScanCommand, Result<Success>> _cancelLibraryScanCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpoint"/> class.
    /// </summary>
    /// <param name="cancelLibraryScanCommandHandler">Injected service for handling cancel library scan commands.</param>
    public CancelLibraryScanEndpoint(ICommandHandler<CancelLibraryScanCommand, Result<Success>> cancelLibraryScanCommandHandler)
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
    /// Cancels a previously started scan of a media library.
    /// </summary>
    /// <param name="request">The request containing the media library and the scan to cancel.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CancelLibraryScanRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _cancelLibraryScanCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
