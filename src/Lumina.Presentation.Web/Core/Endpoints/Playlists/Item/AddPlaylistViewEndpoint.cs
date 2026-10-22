#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Playlists;
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
/// API endpoint for the <c>/{culture}/playlists/item</c> route.
/// </summary>
public class AddPlaylistViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Playlists.Item";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "playlists/item";
    // A new playlist has no entries yet, but its serialized page still carries the shape the page script expects.
    private const int DEFAULT_ENTRY_PAGE_SIZE = 200;

    private static readonly JsonSerializerOptions s_camelCaseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IAntiforgery _antiforgery;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistViewEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public AddPlaylistViewEndpoint(IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
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
        Routes(WebRoutes.Playlists.ADD);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
    }

    /// <summary>
    /// Displays the page that creates a playlist of the current user, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The page creates a playlist of the authenticated user, so no intermediary may store it, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";

        // The page submits through the form anti-forgery token, so a fresh token is issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);
        PlaylistDto playlist = new(
            Id: Guid.Empty,
            Name: string.Empty,
            Description: null,
            Type: PlaylistType.Music,
            Entries: new PaginatedDto<PlaylistEntryDto>(
                Data: [],
                CurrentPage: 1,
                PerPage: DEFAULT_ENTRY_PAGE_SIZE,
                Count: 0,
                NumberOfPages: 0),
            CreatedOnUtc: default,
            UpdatedOnUtc: null
        );
        string title = _localizer["CreatePlaylist"].Value;

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["isNew"] = true,
                ["playlistId"] = playlist.Id.ToString(),
                ["playlistJson"] = JsonSerializer.Serialize(playlist, s_camelCaseJsonOptions),
                ["entriesPerPage"] = DEFAULT_ENTRY_PAGE_SIZE,
                ["createPlaylistUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.CREATE_PLAYLIST) ?? string.Empty,
                ["itemUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Playlists.ITEM, new { playlistId = default(Guid) }) ?? string.Empty,
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
            return View("/Core/Views/Playlists/Item.cshtml", playlist, new Dictionary<string, object?> { ["Title"] = title, ["IsNew"] = true });

        return View("/Core/Views/Shared/_ThemedView.cshtml", new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script), new Dictionary<string, object?> { ["Title"] = title, ["IsNew"] = true });
    }
}
