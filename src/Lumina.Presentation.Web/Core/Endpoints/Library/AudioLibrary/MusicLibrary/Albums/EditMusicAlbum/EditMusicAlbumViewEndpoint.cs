#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.EditMusicAlbum;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}</c> route.
/// </summary>
public class EditMusicAlbumViewEndpoint : BaseEndpoint<GetMusicAlbumViewRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Library.AudioLibrary.MusicLibrary.Albums.Item";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "library/audio-library/music-library/albums/item";

    private static readonly JsonSerializerOptions s_camelCaseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IApiHttpClient _apiHttpClient;
    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IAntiforgery _antiforgery;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicAlbumViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public EditMusicAlbumViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
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
        Routes(WebRoutes.Music.ALBUM);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Displays the music album details and editing view, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request containing the Id of the album to view.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMusicAlbumViewRequest request, CancellationToken cancellationToken)
    {
        // The view needs the full details of the album to show its current values, so they are fetched from the API up front; the route
        // values are normalized to Guids before they are substituted into the upstream URL, so that a crafted route value can never escape
        // its URL segment, and an unparseable id is reported by the API as a missing album id.
        request = request with { ArtistId = Guid.TryParse(request.ArtistId, out Guid artistId) ? artistId.ToString() : Guid.Empty.ToString() };
        request = request with { AlbumId = Guid.TryParse(request.AlbumId, out Guid albumId) ? albumId.ToString() : Guid.Empty.ToString() };
        MusicAlbumDto album = await _apiHttpClient.GetAsync<MusicAlbumDto>(ApiRoutes.Music.GET_ALBUM.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{artistId}", request.ArtistId).Replace("{albumId}", request.AlbumId), cancellationToken).ConfigureAwait(false);
        string title = album.Metadata?.Title ?? string.Empty;

        // The themed view is a regular page that submits its own requests, so a fresh anti-forgery token must be issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["artistId"] = album.ArtistId.ToString(),
                ["albumId"] = album.Id.ToString(),
                ["libraryId"] = album.LibraryId.ToString(),
                ["antiforgeryToken"] = antiforgeryTokens.RequestToken,
                ["saveUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.SAVE_ALBUM, new { artistId = default(Guid), albumId = default(Guid) }) ?? string.Empty,
                ["deleteUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.DELETE_ALBUM, new { artistId = default(Guid), albumId = default(Guid) }) ?? string.Empty,
                ["tracksUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.GET_ALBUM_TRACKS, new { artistId = default(Guid), albumId = default(Guid) }) ?? string.Empty,
                ["trackUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.TRACK, new { artistId = default(Guid), albumId = default(Guid), trackId = default(Guid) }) ?? string.Empty,
                ["coverUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.UPDATE_ALBUM_COVER, new { artistId = default(Guid), albumId = default(Guid) }) ?? string.Empty,
                ["backUrl"] = (_urlService.GetAbsoluteUrl(WebRoutes.Music.ARTIST, new { artistId = album.ArtistId }) ?? string.Empty) + "?libraryId=" + album.LibraryId,
                ["albumJson"] = JsonSerializer.Serialize(album, s_camelCaseJsonOptions),
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer),
                ["stringsJson"] = JsonSerializer.Serialize(ThemePageDataFactory.CreateLocalizedStrings(_localizer), s_camelCaseJsonOptions)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
        {
            Dictionary<string, object?> viewData = new()
            {
                ["Title"] = title
            };
            return View("/Core/Views/Library/AudioLibrary/MusicLibrary/Albums/Item.cshtml", album, viewData);
        }

        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = title });
    }
}
