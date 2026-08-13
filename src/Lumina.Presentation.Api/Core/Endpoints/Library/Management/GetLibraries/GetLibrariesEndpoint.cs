#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly IQueryHandler<GetLibrariesQuery, ErrorOr<LibraryResponse[]>> _getLibrariesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpoint"/> class.
    /// </summary>
    /// <param name="getLibrariesQueryHandler">Injected service for handling get libraries queries.</param>
    public GetLibrariesEndpoint(IQueryHandler<GetLibrariesQuery, ErrorOr<LibraryResponse[]>> getLibrariesQueryHandler)
    {
        _getLibrariesQueryHandler = getLibrariesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<LibraryResponse[]> result = await _getLibrariesQueryHandler.HandleAsync(new GetLibrariesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
