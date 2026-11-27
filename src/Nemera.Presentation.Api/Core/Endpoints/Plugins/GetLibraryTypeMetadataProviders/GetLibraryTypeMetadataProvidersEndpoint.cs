#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;
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

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeMetadataProviders;

/// <summary>
/// API endpoint for the <c>/libraries/metadata-providers/{libraryType}</c> route.
/// </summary>
public class GetLibraryTypeMetadataProvidersEndpoint : BaseEndpoint<GetLibraryTypeMetadataProvidersRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryTypeMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>> _getLibraryTypeMetadataProvidersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryTypeMetadataProvidersQueryHandler">Injected service for handling get library type metadata providers queries.</param>
    public GetLibraryTypeMetadataProvidersEndpoint(IQueryHandler<GetLibraryTypeMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>> getLibraryTypeMetadataProvidersQueryHandler)
    {
        _getLibraryTypeMetadataProvidersQueryHandler = getLibraryTypeMetadataProvidersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_TYPE_METADATA_PROVIDERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the metadata providers available for the media library type provided in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the media library type whose available metadata providers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryTypeMetadataProvidersRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _getLibraryTypeMetadataProvidersQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
