#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.ReorderLibraryMetadataProviders;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/metadata-providers/reorder</c> route.
/// </summary>
public class ReorderLibraryMetadataProvidersEndpoint : BaseEndpoint<ReorderLibraryMetadataProvidersRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public ReorderLibraryMetadataProvidersEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes(ApiRoutes.Libraries.REORDER_LIBRARY_METADATA_PROVIDERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Reorders the metadata providers of the media library identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library and the plugin Ids in the new order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ReorderLibraryMetadataProvidersRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
