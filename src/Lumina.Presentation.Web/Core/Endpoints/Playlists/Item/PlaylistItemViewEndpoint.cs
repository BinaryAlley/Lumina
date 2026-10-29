#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
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

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.Item;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}</c> route.
/// </summary>
public class PlaylistItemViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Playlists.Item";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "playlists/item";
    // The page renders one page at a time; the client asks for the next page through getPlaylistUrl.
    private const int ENTRY_PAGE_SIZE = 200;

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
    /// Initializes a new instance of the <see cref="PlaylistItemViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public PlaylistItemViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
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
        Routes(WebRoutes.Playlists.ITEM);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
    }

    /// <summary>
    /// Displays a playlist of the current user, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The page shows a playlist of the authenticated user, so no intermediary may store it, and the response varies by the sign in cookie.
        SetCacheControl("private, no-store");
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The route value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted value can never escape
        // its URL segment, and an unparseable id is reported by the API as a missing playlist id.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        PlaylistDto playlist = await _apiHttpClient
            .GetAsync<PlaylistDto>(
                ApiRoutes.Playlists.GET_PLAYLIST.Replace("{playlistId}", playlistId) + $"?currentPage=1&perPage={ENTRY_PAGE_SIZE}",
                cancellationToken).ConfigureAwait(false);

        // The page edits, reorders and deletes the playlist, which submit through the form anti-forgery token, so a fresh token is issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = playlist.Name,
            Description = playlist.Description ?? string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["isNew"] = false,
                ["playlistId"] = playlist.Id.ToString(),
                ["playlistJson"] = JsonSerializer.Serialize(playlist, s_camelCaseJsonOptions),
                ["entriesPerPage"] = ENTRY_PAGE_SIZE,
                ["createPlaylistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.CREATE_PLAYLIST) ?? string.Empty,
                ["itemUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.ITEM, new { playlistId = default(Guid) }) ?? string.Empty,
                ["getPlaylistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.GET_PLAYLIST, new { playlistId = playlist.Id }) ?? string.Empty,
                ["updatePlaylistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.UPDATE_PLAYLIST, new { playlistId = playlist.Id }) ?? string.Empty,
                ["deletePlaylistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.DELETE_PLAYLIST, new { playlistId = playlist.Id }) ?? string.Empty,
                ["addEntryUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.ADD_PLAYLIST_ENTRY, new { playlistId = playlist.Id }) ?? string.Empty,
                ["removeEntryUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.REMOVE_PLAYLIST_ENTRY, new { playlistId = playlist.Id, playlistEntryId = default(Guid) }) ?? string.Empty,
                ["moveEntryUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.MOVE_PLAYLIST_ENTRY, new { playlistId = playlist.Id, playlistEntryId = default(Guid) }) ?? string.Empty,
                ["getEnabledLibrariesUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.GET_ENABLED_LIBRARIES) ?? string.Empty,
                ["getArtistsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTISTS) ?? string.Empty,
                ["getArtistAlbumsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.GET_ARTIST_ALBUMS, new { artistId = default(Guid) }) ?? string.Empty,
                ["getAlbumTracksUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.GET_ALBUM_TRACKS, new { artistId = default(Guid), albumId = default(Guid) }) ?? string.Empty,
                ["indexUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.INDEX) ?? string.Empty,
                ["antiforgeryToken"] = antiforgeryTokens.RequestToken,
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Playlists/Item.cshtml", playlist, new Dictionary<string, object?> { ["Title"] = playlist.Name, ["IsNew"] = false });

        return View("/Core/Views/Shared/_ThemedView.cshtml", new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script), new Dictionary<string, object?> { ["Title"] = playlist.Name, ["IsNew"] = false });
    }
}
