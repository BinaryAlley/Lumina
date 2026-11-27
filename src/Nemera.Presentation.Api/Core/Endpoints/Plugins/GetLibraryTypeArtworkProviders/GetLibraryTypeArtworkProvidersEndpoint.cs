#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Nemera.Contracts.Requests.Plugins;
using Nemera.Contracts.Responses.Plugins;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.Management;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeArtworkProviders;

/// <summary>
/// API endpoint for the <c>/libraries/artwork-providers/{libraryType}</c> route.
/// </summary>
public class GetLibraryTypeArtworkProvidersEndpoint : BaseEndpoint<GetLibraryTypeArtworkProvidersRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryTypeArtworkProvidersQuery, Result<IReadOnlyList<LibraryArtworkProviderResponse>>> _getLibraryTypeArtworkProvidersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryTypeArtworkProvidersQueryHandler">Injected service for handling get library type artwork providers queries.</param>
    public GetLibraryTypeArtworkProvidersEndpoint(IQueryHandler<GetLibraryTypeArtworkProvidersQuery, Result<IReadOnlyList<LibraryArtworkProviderResponse>>> getLibraryTypeArtworkProvidersQueryHandler)
    {
        _getLibraryTypeArtworkProvidersQueryHandler = getLibraryTypeArtworkProvidersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_TYPE_ARTWORK_PROVIDERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the artwork providers available for the media library type provided in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the media library type whose available artwork providers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryTypeArtworkProvidersRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _getLibraryTypeArtworkProvidersQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
