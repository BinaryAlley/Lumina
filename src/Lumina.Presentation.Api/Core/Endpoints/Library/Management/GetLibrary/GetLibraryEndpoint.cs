#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibrary;

/// <summary>
/// API endpoint for the <c>/libraries/{id}</c> route.
/// </summary>
public class GetLibraryEndpoint : BaseEndpoint<GetLibraryRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryQuery, ErrorOr<LibraryResponse>> _getLibraryQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryQueryHandler">Injected service for handling get library queries.</param>
    public GetLibraryEndpoint(IQueryHandler<GetLibraryQuery, ErrorOr<LibraryResponse>> getLibraryQueryHandler)
    {
        _getLibraryQueryHandler = getLibraryQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_BY_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets a library by Id.
    /// </summary>
    /// <param name="request">The request containing the id of the library to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<LibraryResponse> result = await _getLibraryQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
