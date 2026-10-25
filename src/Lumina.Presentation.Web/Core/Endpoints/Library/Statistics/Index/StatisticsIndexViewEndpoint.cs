#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics</c> route.
/// </summary>
public class StatisticsIndexViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Library.Statistics.Index";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "library/statistics/index";

    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public StatisticsIndexViewEndpoint(IUrlService urlService, ThemePageRenderer themePageRenderer, IStringLocalizerFactory stringLocalizerFactory)
    {
        _urlService = urlService;
        _themePageRenderer = themePageRenderer;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.LibraryStatistics.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
    }

    /// <summary>
    /// Displays the statistics of the current user, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The page shows the statistics of the authenticated user, so no intermediary may store it, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        string title = _localizer["Title"].Value;

        // The track link template carries the all-zero Guid placeholders of its route values, in route order (artist, album and track), which the
        // page replaces one at a time. The detail page of a track expects the Id of its media library in the libraryId query parameter.
        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["getRecentlyPlayedUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_RECENTLY_PLAYED) ?? string.Empty,
                ["getMostPlayedUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_MOST_PLAYED) ?? string.Empty,
                ["getStatisticsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_MUSIC_PLAY_STATISTICS) ?? string.Empty,
                ["trackUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Music.TRACK, new { artistId = default(Guid), albumId = default(Guid), trackId = default(Guid) }) ?? string.Empty,
                ["culture"] = Culture,
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Library/Statistics/Index.cshtml", viewData: new Dictionary<string, object?> { ["Title"] = title });

        return View("/Core/Views/Shared/_ThemedView.cshtml", new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script), new Dictionary<string, object?> { ["Title"] = title });
    }
}
