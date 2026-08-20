#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Themes.ThemeAssets;

/// <summary>
/// API endpoint for the <c>/theme-assets/{themeId}/{path}</c> route.
/// </summary>
public class ThemeAssetsEndpoint : BaseEndpoint<GetThemeAssetRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAssetsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ThemeAssetsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Themes.THEME_ASSETS);
        AllowAnonymous();
        DontAutoTag();
        Options(options => options.WithTags("Themes"));
    }

    /// <summary>
    /// Serves the asset of the theme stored in <paramref name="request"/>, fetched from the remote API.
    /// </summary>
    /// <param name="request">The request containing the theme and the path of the asset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThemeAssetRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ThemeId) || string.IsNullOrWhiteSpace(request.Path))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The theme id and the asset path are required.");

        BlobDataDto blob = await _apiHttpClient.GetBlobAsync(
            ApiRoutes.Themes.GET_THEME_ASSET.Replace("{themeId}", request.ThemeId).Replace("{*assetPath}", request.Path), cancellationToken).ConfigureAwait(false);
        return Results.File(blob.Data, blob.ContentType);
    }
}
