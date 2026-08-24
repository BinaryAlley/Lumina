#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryArtworkProviders;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artwork-providers</c> route.
/// </summary>
public class GetLibraryArtworkProvidersEndpoint : BaseEndpoint<GetLibraryArtworkProvidersRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryArtworkProvidersQuery, Result<IReadOnlyList<LibraryArtworkProviderResponse>>> _getLibraryArtworkProvidersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryArtworkProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryArtworkProvidersQueryHandler">Injected service for handling get library artwork providers queries.</param>
    public GetLibraryArtworkProvidersEndpoint(IQueryHandler<GetLibraryArtworkProvidersQuery, Result<IReadOnlyList<LibraryArtworkProviderResponse>>> getLibraryArtworkProvidersQueryHandler)
    {
        _getLibraryArtworkProvidersQueryHandler = getLibraryArtworkProvidersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_ARTWORK_PROVIDERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the artwork providers configured for the media library identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose artwork providers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryArtworkProvidersRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _getLibraryArtworkProvidersQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
