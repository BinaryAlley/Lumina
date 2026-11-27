#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetArtworkProviders;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-get-artwork-providers/{libraryId}</c> route.
/// </summary>
public class GetArtworkProvidersEndpoint : BaseEndpoint<GetArtworkProvidersRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtworkProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetArtworkProvidersEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.LibraryManagement.GET_ARTWORK_PROVIDERS);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Retrieves the artwork providers of a media library.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose artwork providers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetArtworkProvidersRequest request, CancellationToken cancellationToken)
    {
        LibraryArtworkProviderDto[] response = await _apiHttpClient.GetAsync<LibraryArtworkProviderDto[]>(ApiRoutes.Libraries.GET_LIBRARY_ARTWORK_PROVIDERS.Replace("{libraryId}", request.LibraryId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
