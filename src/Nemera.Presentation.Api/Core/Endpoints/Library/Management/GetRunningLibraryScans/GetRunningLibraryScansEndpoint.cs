#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.Management;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// API endpoint for the <c>/libraries/scans/running</c> route.
/// </summary>
public class GetRunningLibraryScansEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetRunningLibraryScansQuery, Result<IEnumerable<MediaLibraryScanProgressResponse>>> _getRunningLibraryScansQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpoint"/> class.
    /// </summary>
    /// <param name="getRunningLibraryScansQueryHandler">Injected service for handling get running library scans queries.</param>
    public GetRunningLibraryScansEndpoint(IQueryHandler<GetRunningLibraryScansQuery, Result<IEnumerable<MediaLibraryScanProgressResponse>>> getRunningLibraryScansQueryHandler)
    {
        _getRunningLibraryScansQueryHandler = getRunningLibraryScansQueryHandler;
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
    /// <param name="_">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _getRunningLibraryScansQueryHandler.HandleAsync(new GetRunningLibraryScansQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
