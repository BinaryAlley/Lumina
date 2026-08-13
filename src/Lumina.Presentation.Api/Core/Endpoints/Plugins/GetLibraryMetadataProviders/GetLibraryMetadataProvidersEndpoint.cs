#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryMetadataProviders;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/metadata-providers</c> route.
/// </summary>
public class GetLibraryMetadataProvidersEndpoint : BaseEndpoint<GetLibraryMetadataProvidersRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryMetadataProvidersQuery, ErrorOr<IReadOnlyList<LibraryMetadataProviderResponse>>> _getLibraryMetadataProvidersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryMetadataProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryMetadataProvidersQueryHandler">Injected service for handling get library metadata providers queries.</param>
    public GetLibraryMetadataProvidersEndpoint(IQueryHandler<GetLibraryMetadataProvidersQuery, ErrorOr<IReadOnlyList<LibraryMetadataProviderResponse>>> getLibraryMetadataProvidersQueryHandler)
    {
        _getLibraryMetadataProvidersQueryHandler = getLibraryMetadataProvidersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Libraries.GET_LIBRARY_METADATA_PROVIDERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the metadata providers configured for the media library identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose metadata providers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryMetadataProvidersRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _getLibraryMetadataProvidersQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
