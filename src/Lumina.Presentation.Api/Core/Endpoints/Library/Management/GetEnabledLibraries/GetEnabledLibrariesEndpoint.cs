#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// API endpoint for the <c>/libraries/enabled</c> route.
/// </summary>
public class GetEnabledLibrariesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetEnabledLibrariesQuery, Result<LibraryResponse[]>> _getEnabledLibrariesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpoint"/> class.
    /// </summary>
    /// <param name="getEnabledLibrariesQueryHandler">Injected service for handling get enabled libraries queries.</param>
    public GetEnabledLibrariesEndpoint(IQueryHandler<GetEnabledLibrariesQuery, Result<LibraryResponse[]>> getEnabledLibrariesQueryHandler)
    {
        _getEnabledLibrariesQueryHandler = getEnabledLibrariesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_ENABLED_LIBRARIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of enabled media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<LibraryResponse[]> result = await _getEnabledLibrariesQueryHandler.HandleAsync(new GetEnabledLibrariesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
