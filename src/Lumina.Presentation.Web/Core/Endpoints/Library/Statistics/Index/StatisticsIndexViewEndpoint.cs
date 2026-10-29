#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
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
using System.Security.Claims;
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
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    /// <param name="authorizationService">Injected service used to resolve the role of the current user.</param>
    public StatisticsIndexViewEndpoint(IUrlService urlService, ThemePageRenderer themePageRenderer, IStringLocalizerFactory stringLocalizerFactory, IAuthorizationService authorizationService)
    {
        _urlService = urlService;
        _themePageRenderer = themePageRenderer;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
        _authorizationService = authorizationService;
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
    /// Displays the statistics page, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The page shows the statistics of the authenticated user, so no intermediary may store it, and the response varies by the sign in cookie.
        SetCacheControl("private, no-store");
        HttpContext.Response.Headers["Vary"] = "Cookie";
        string title = _localizer["Title"].Value;
        bool isAdmin = await _authorizationService.IsInRoleAsync("Admin", cancellationToken).ConfigureAwait(false);
        string? currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? parsedCurrentUserId = Guid.TryParse(currentUserId, out Guid userId) ? userId : null;

        StatisticsPageDto page = new(
            Title: title,
            IsAdmin: isAdmin,
            CurrentUserId: parsedCurrentUserId,
            Culture: Culture,
            GetStatisticsUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_STATISTICS) ?? string.Empty,
            GetAllUsersStatisticsUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_ALL_USERS_STATISTICS) ?? string.Empty,
            GetRecentlyConsumedUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_RECENTLY_CONSUMED) ?? string.Empty,
            GetMostConsumedUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_MOST_CONSUMED) ?? string.Empty,
            GetTagConsumptionUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_TAG_CONSUMPTION) ?? string.Empty,
            GetGenreConsumptionUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_GENRE_CONSUMPTION) ?? string.Empty,
            GetLibrariesUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_LIBRARIES) ?? string.Empty,
            GetUsersUrl: isAdmin ? _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.GET_USERS) ?? string.Empty : string.Empty,
            DeleteConsumptionItemHistoryUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.DELETE_CONSUMPTION_ITEM_HISTORY) ?? string.Empty,
            ClearConsumptionHistoryUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.CLEAR_CONSUMPTION_HISTORY) ?? string.Empty,
            ClearResumePointsUrl: _urlService.GetAbsoluteUrl(WebRoutes.LibraryStatistics.CLEAR_RESUME_POINTS) ?? string.Empty
        );

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["getStatisticsUrl"] = page.GetStatisticsUrl,
                ["getAllUsersStatisticsUrl"] = page.GetAllUsersStatisticsUrl,
                ["getRecentlyConsumedUrl"] = page.GetRecentlyConsumedUrl,
                ["getMostConsumedUrl"] = page.GetMostConsumedUrl,
                ["getTagConsumptionUrl"] = page.GetTagConsumptionUrl,
                ["getGenreConsumptionUrl"] = page.GetGenreConsumptionUrl,
                ["getLibrariesUrl"] = page.GetLibrariesUrl,
                ["getUsersUrl"] = page.GetUsersUrl,
                ["deleteConsumptionItemHistoryUrl"] = page.DeleteConsumptionItemHistoryUrl,
                ["clearConsumptionHistoryUrl"] = page.ClearConsumptionHistoryUrl,
                ["clearResumePointsUrl"] = page.ClearResumePointsUrl,
                ["culture"] = page.Culture,
                ["isAdmin"] = page.IsAdmin,
                ["currentUserId"] = page.CurrentUserId,
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Library/Statistics/Index.cshtml", page, new Dictionary<string, object?> { ["Title"] = title });

        return View("/Core/Views/Shared/_ThemedView.cshtml", new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script), new Dictionary<string, object?> { ["Title"] = title });
    }
}
