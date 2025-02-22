#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// API endpoint for the <c>/libraries/scans/running</c> route.
/// </summary>
public class GetRunningLibraryScansEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetRunningLibraryScansEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_RUNNING_LIBRARIES_SCAN);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of ongoing media libraries scans.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sender.Send(new GetRunningLibraryScansQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
