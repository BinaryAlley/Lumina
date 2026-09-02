#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books</c> route.
/// </summary>
public class BooksIndexViewEndpoint : BaseEndpoint<GetBooksViewRequest, IResult>
{
    // the resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it
    // with the configured ResourcesPath, so "Lumina.Presentation.Web.Views.X" resolves to the embedded
    // "Lumina.Presentation.Web.Core.Resources.Views.X" resource; including "Core" here would insert it twice
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Library.WrittenContentLibrary.BookLibrary.Books.Index";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // the page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope
    private const string VIEW_PAGE_KEY = "library/written-content-library/book-library/books/index";

    private readonly IApiHttpClient _apiHttpClient;
    private readonly IUrlService _urlService;
    private readonly ThemePageRenderer _themePageRenderer;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public BooksIndexViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService, ThemePageRenderer themePageRenderer, IStringLocalizerFactory stringLocalizerFactory)
    {
        _apiHttpClient = apiHttpClient;
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
        Routes(WebRoutes.Books.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Displays the books browsing view, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose books are browsed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBooksViewRequest request, CancellationToken cancellationToken)
    {
        if (request.LibraryId is null)
            return Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!);
        LibraryDto library = await _apiHttpClient.GetAsync<LibraryDto>(ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", request.LibraryId.Value.ToString()), cancellationToken).ConfigureAwait(false);

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = library.Title ?? string.Empty,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["libraryId"] = library.Id?.ToString() ?? request.LibraryId.Value.ToString(),
                ["itemsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.GET_LIBRARY_ITEMS) ?? string.Empty,
                ["settingsUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Settings.GET_USER_SETTINGS) ?? string.Empty,
                ["readUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.READ, new { bookId = default(Guid) }) ?? string.Empty,
                ["availabilityUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.GET_READING_AVAILABILITY, new { bookId = default(Guid) }) ?? string.Empty,
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer)
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Library/WrittenContentLibrary/BookLibrary/Books/Index.cshtml", library);

        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = pageModel.Title });
    }
}
