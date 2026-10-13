#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists</c> route.
/// </summary>
public class MusicArtistsIndexViewEndpoint : BaseEndpoint<GetMusicArtistsViewRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it
    // with the configured ResourcesPath, so "Lumina.Presentation.Web.Views.X" resolves to the embedded
    // "Lumina.Presentation.Web.Core.Resources.Views.X" resource; including "Core" here would insert it twice.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Library.AudioLibrary.MusicLibrary.Artists.Index";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "library/audio-library/music-library/artists/index";

    private readonly IApiHttpClient _apiHttpClient;
    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IAntiforgery _antiforgery;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtistsIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public MusicArtistsIndexViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
    {
        _apiHttpClient = apiHttpClient;
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
        Routes(WebRoutes.Music.ARTISTS_INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Displays the music artists browsing view, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose artists are browsed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMusicArtistsViewRequest request, CancellationToken cancellationToken)
    {
        if (request.LibraryId is null)
            return Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!);
        LibraryDto library = await _apiHttpClient.GetAsync<LibraryDto>(ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", request.LibraryId.Value.ToString()), cancellationToken).ConfigureAwait(false);

        // The page exposes delete actions, which submit through the form anti-forgery token, so a fresh token is issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = library.Title ?? string.Empty,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["libraryId"] = library.Id?.ToString() ?? request.LibraryId.Value.ToString(),
                ["antiforgeryToken"] = antiforgeryTokens.RequestToken,
                ["itemsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTISTS) ?? string.Empty,
                ["settingsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Settings.GET_USER_SETTINGS) ?? string.Empty,
                ["artistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.ARTIST, new { artistId = default(Guid) }) ?? string.Empty,
                ["deleteUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.DELETE_ARTIST, new { artistId = default(Guid) }) ?? string.Empty,
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Library/AudioLibrary/MusicLibrary/Artists/Index.cshtml", library);

        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = pageModel.Title });
    }
}
