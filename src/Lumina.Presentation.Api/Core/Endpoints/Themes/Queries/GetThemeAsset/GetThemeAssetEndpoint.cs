#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeAsset;

/// <summary>
/// API endpoint for the <c>/themes/{themeId}/assets/{assetPath}</c> route.
/// </summary>
public class GetThemeAssetEndpoint : BaseEndpoint<GetThemeAssetRequest, IResult>
{
    private readonly IQueryHandler<GetThemeAssetQuery, Result<ThemeAssetResponse>> _getThemeAssetQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetEndpoint"/> class.
    /// </summary>
    /// <param name="getThemeAssetQueryHandler">Injected service for handling get theme asset queries.</param>
    public GetThemeAssetEndpoint(IQueryHandler<GetThemeAssetQuery, Result<ThemeAssetResponse>> getThemeAssetQueryHandler)
    {
        _getThemeAssetQueryHandler = getThemeAssetQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Themes.GET_THEME_ASSET);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the asset of the theme stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme and the path of the asset to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThemeAssetRequest request, CancellationToken cancellationToken)
    {
        Result<ThemeAssetResponse> result = await _getThemeAssetQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.File(success.Bytes, success.ContentType), Problem);
    }
}
