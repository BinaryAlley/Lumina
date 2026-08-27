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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetArtworkProviderEnabled;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-set-artwork-provider-enabled</c> route.
/// </summary>
public class SetArtworkProviderEnabledEndpoint : BaseEndpoint<SetLibraryArtworkProviderEnabledRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetArtworkProviderEnabledEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SetArtworkProviderEnabledEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.SET_ARTWORK_PROVIDER_ENABLED);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Enables or disables an artwork provider of a media library.
    /// </summary>
    /// <param name="request">The request containing the Ids of the media library and of the plugin, and whether the provider is enabled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetLibraryArtworkProviderEnabledRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutAsync<Web.Common.Requests.Common.EmptyRequest, SetLibraryArtworkProviderEnabledRequest>(ApiRoutes.Libraries.SET_LIBRARY_ARTWORK_PROVIDER_ENABLED.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{pluginId}", request.PluginId.ToString()), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
