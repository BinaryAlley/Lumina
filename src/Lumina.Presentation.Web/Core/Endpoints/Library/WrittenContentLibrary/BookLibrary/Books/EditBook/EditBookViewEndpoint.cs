#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.EditBook;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{id}</c> route.
/// </summary>
public class EditBookViewEndpoint : BaseEndpoint<GetBookRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath,
    // so "Lumina.Presentation.Web.Views.X" resolves to the embedded "Lumina.Presentation.Web.Core.Resources.Views.X" resource; including "Core" here would insert it twice.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Library.WrittenContentLibrary.BookLibrary.Books.Item";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "library/written-content-library/book-library/books/item";

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
    /// Initializes a new instance of the <see cref="EditBookViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public EditBookViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
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
        Routes(WebRoutes.Books.EDIT_BOOK);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Displays the book editing view, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request containing the Id of the book to view.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookRequest request, CancellationToken cancellationToken)
    {
        // The view needs the full details of the book to show its current values, so they are fetched from the API up front; the route
        // value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted route value can never escape
        // its URL segment, and an unparseable id is reported by the API as a missing book id.
        request = request with { Id = Guid.TryParse(request.Id, out Guid bookId) ? bookId.ToString() : Guid.Empty.ToString() };
        BookDetailsDto book = await _apiHttpClient.GetAsync<BookDetailsDto>(ApiRoutes.Books.GET_BOOK.Replace("{id}", request.Id), cancellationToken).ConfigureAwait(false);
        string title = book.Metadata?.Title ?? string.Empty;

        // The themed view is a regular page that submits its own requests, so a fresh anti-forgery token must be issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);

        // The theme template receives the book data, the URLs of the actions of the view and the localized strings as page data,
        // serialized as JSON so that the template can bind them without executing server side code of its own.
        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["bookId"] = book.Id.ToString(),
                ["libraryId"] = book.LibraryId.ToString(),
                ["antiforgeryToken"] = antiforgeryTokens.RequestToken,
                ["saveUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.SAVE_BOOK, new { id = default(Guid) }) ?? string.Empty,
                ["coverUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.UPDATE_BOOK_COVER, new { id = default(Guid) }) ?? string.Empty,
                ["backUrl"] = (_urlService.GetAbsoluteUrl(WebRoutes.Books.INDEX) ?? string.Empty) + "?libraryId=" + book.LibraryId,
                ["bookJson"] = JsonSerializer.Serialize(book, s_camelCaseJsonOptions),
                ["strings"] = ThemePageDataFactory.CreateLocalizedStrings(_localizer),
                ["stringsJson"] = JsonSerializer.Serialize(ThemePageDataFactory.CreateLocalizedStrings(_localizer), s_camelCaseJsonOptions)
            }
        };

        // Themed rendering is best effort: when the active theme does not provide the item template, the plain Razor fallback is rendered instead.
        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
        {
            Dictionary<string, object?> viewData = new()
            {
                ["Title"] = title
            };
            return View("/Core/Views/Library/WrittenContentLibrary/BookLibrary/Books/Item.cshtml", book, viewData);
        }

        // On a healthy site the page is composed by the theme, so the theme content and script are wrapped in the shared themed view.
        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = title });
    }
}
