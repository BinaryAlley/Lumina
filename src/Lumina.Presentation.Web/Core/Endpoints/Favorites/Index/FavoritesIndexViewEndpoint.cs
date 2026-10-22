#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/favorites</c> route.
/// </summary>
public class FavoritesIndexViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Favorites.Index";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "favorites/index";

    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IAntiforgery _antiforgery;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public FavoritesIndexViewEndpoint(IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
    {
        _urlService = urlService;
        _themePageRenderer = themePageRenderer;
        _antiforgery = antiforgery;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Favorites.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Favorites"));
    }

    /// <summary>
    /// Displays the favorites of the current user across media kinds, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The page shows the favorites of the authenticated user, so no intermediary may store it, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The page removes favorites, which submit through the form anti-forgery token, so a fresh token is issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);
        string title = _localizer["Title"].Value;

        // The media item link templates carry the all-zero Guid placeholders of their route values, in route order, which the page replaces
        // one at a time. The remove template keeps its named mediaKind and mediaItemId route placeholders instead, so those two values do not
        // depend on the segment order. The detail pages of the media items expect the Id of their media library in the libraryId query
        // parameter, which the page appends to the link from the favorite item.
        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["getFavoritesUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Favorites.GET_FAVORITES) ?? string.Empty,
                ["getFavoriteStatusUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Favorites.GET_FAVORITE_STATUS) ?? string.Empty,
                ["addFavoriteUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Favorites.ADD_FAVORITE) ?? string.Empty,
                ["removeFavoriteUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Favorites.REMOVE_FAVORITE) ?? string.Empty,
                ["artistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.ARTIST, new { artistId = default(Guid) }) ?? string.Empty,
                ["albumUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.ALBUM, new { artistId = default(Guid), albumId = default(Guid) }) ?? string.Empty,
                ["trackUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.TRACK, new { artistId = default(Guid), albumId = default(Guid), trackId = default(Guid) }) ?? string.Empty,
                ["bookUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.EDIT_BOOK, new { bookId = default(Guid) }) ?? string.Empty,
                ["antiforgeryToken"] = antiforgeryTokens.RequestToken,
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Favorites/Index.cshtml", viewData: new Dictionary<string, object?> { ["Title"] = title });

        return View("/Core/Views/Shared/_ThemedView.cshtml", new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script), new Dictionary<string, object?> { ["Title"] = title });
    }
}
