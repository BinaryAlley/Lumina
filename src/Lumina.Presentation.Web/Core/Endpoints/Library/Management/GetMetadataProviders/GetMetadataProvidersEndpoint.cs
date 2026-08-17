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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetMetadataProviders;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-get-metadata-providers/{libraryId}</c> route.
/// </summary>
public class GetMetadataProvidersEndpoint : BaseEndpoint<GetMetadataProvidersRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMetadataProvidersEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetMetadataProvidersEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.GET_METADATA_PROVIDERS);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Retrieves the metadata providers of a media library.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose metadata providers are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMetadataProvidersRequest request, CancellationToken cancellationToken)
    {
        LibraryMetadataProviderDto[] response = await _apiHttpClient.GetAsync<LibraryMetadataProviderDto[]>(ApiRoutes.Libraries.GET_LIBRARY_METADATA_PROVIDERS.Replace("{libraryId}", request.LibraryId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
