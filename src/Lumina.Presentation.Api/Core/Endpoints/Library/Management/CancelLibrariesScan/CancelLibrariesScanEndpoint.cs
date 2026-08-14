#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.CancelLibrariesScan;

/// <summary>
/// API endpoint for the <c>/libraries/scans/cancel</c> route.
/// </summary>
public class CancelLibrariesScanEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly Lumina.Application.Common.CQRS.ICommandHandler<CancelLibrariesScanCommand, Result<Success>> _cancelLibrariesScanCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanEndpoint"/> class.
    /// </summary>
    /// <param name="cancelLibrariesScanCommandHandler">Injected service for handling cancel libraries scan commands.</param>
    public CancelLibrariesScanEndpoint(Lumina.Application.Common.CQRS.ICommandHandler<CancelLibrariesScanCommand, Result<Success>> cancelLibrariesScanCommandHandler)
    {
        _cancelLibrariesScanCommandHandler = cancelLibrariesScanCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiRoutes.Libraries.CANCEL_LIBRARIES_SCAN);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Cancels a previously started scan of all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<Success> result = await _cancelLibrariesScanCommandHandler.HandleAsync(new CancelLibrariesScanCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
