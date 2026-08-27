#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ReorderArtworkProviders;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-reorder-artwork-providers</c> route.
/// </summary>
public class ReorderArtworkProvidersEndpoint : BaseEndpoint<ReorderLibraryArtworkProvidersRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderArtworkProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ReorderArtworkProvidersEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.LibraryManagement.REORDER_ARTWORK_PROVIDERS);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Reorders the artwork providers of a media library.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library and the plugin Ids in the new order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ReorderLibraryArtworkProvidersRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutAsync<Web.Common.Requests.Common.EmptyRequest, ReorderLibraryArtworkProvidersRequest>(ApiRoutes.Libraries.REORDER_LIBRARY_ARTWORK_PROVIDERS.Replace("{libraryId}", request.LibraryId.ToString()), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
