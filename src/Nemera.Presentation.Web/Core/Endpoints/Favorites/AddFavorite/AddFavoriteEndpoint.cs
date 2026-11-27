#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.Favorites;
using Nemera.Presentation.Web.Common.Requests.Favorites;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// API endpoint for the <c>/{culture}/favorites/api-add-favorite</c> route.
/// </summary>
public class AddFavoriteEndpoint : BaseEndpoint<AddFavoriteRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public AddFavoriteEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Favorites.ADD_FAVORITE);
        DontAutoTag();
        Options(options => options.WithTags("Favorites"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Marks a media item as a favorite of the current user.
    /// </summary>
    /// <param name="request">The request containing the kind and the Id of the media item to favorite.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddFavoriteRequest request, CancellationToken cancellationToken)
    {
        FavoriteDto favorite = await _apiHttpClient.PostAsync<FavoriteDto, AddFavoriteRequest>(ApiRoutes.Favorites.ADD_FAVORITE, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(favorite);
    }
}
