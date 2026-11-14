#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeletedBooks;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/deleted-items</c> route.
/// </summary>
public class DeletedBooksViewEndpoint : BaseEndpoint<GetBooksViewRequest, IResult>
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Library.WrittenContentLibrary.BookLibrary.Books.DeletedItems";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";
    // The page key mirrors the path of the Razor view under Core/Views, so that theme templates can override it at the page, section or default scope.
    private const string VIEW_PAGE_KEY = "library/written-content-library/book-library/books/deleted-items";

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
    /// Initializes a new instance of the <see cref="DeletedBooksViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="themePageRenderer">Injected service for rendering themed pages.</param>
    /// <param name="antiforgery">Injected service for anti-forgery token handling.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the view resources.</param>
    public DeletedBooksViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService, ThemePageRenderer themePageRenderer, IAntiforgery antiforgery, IStringLocalizerFactory stringLocalizerFactory)
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
        Routes(WebRoutes.Books.DELETED_BOOKS);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Displays the deleted books of a media library, rendered by the active theme with a fallback to the Razor view.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose deleted books are shown.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBooksViewRequest request, CancellationToken cancellationToken)
    {
        if (request.LibraryId is null)
            return Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!);

        LibraryDto library = await _apiHttpClient.GetAsync<LibraryDto>(
            ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", request.LibraryId.Value.ToString()), cancellationToken).ConfigureAwait(false);
        DeletedBookDto[] deletedBooks = await _apiHttpClient.GetAsync<DeletedBookDto[]>(
            ApiRoutes.Books.GET_DELETED_BOOKS.Replace("{libraryId}", request.LibraryId.Value.ToString()), cancellationToken).ConfigureAwait(false);

        // The page restores books, which submit through the form anti-forgery token, so a fresh token must be issued for it.
        AntiforgeryTokenSet antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);

        string title = _localizer["Title"].Value;
        string backUrl = (_urlService.GetAbsoluteUrl(WebRoutes.Books.INDEX) ?? string.Empty) + "?libraryId=" + request.LibraryId.Value;
        Dictionary<string, object?> strings = ThemePageDataFactory.CreateLocalizedStrings(_localizer);
        // The themed page script reads the localized strings through camel case keys, and they are serialized as JSON so that quotes and
        // apostrophes in translations never terminate the JavaScript string literals of the template.
        string stringsJson = JsonSerializer.Serialize(strings.ToDictionary(
            entry => JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
            entry => entry.Value));

        ThemePageDto pageModel = new()
        {
            PageKey = VIEW_PAGE_KEY,
            Title = title,
            Description = string.Empty,
            PageData = new Dictionary<string, object?>
            {
                ["libraryId"] = request.LibraryId.Value.ToString(),
                ["libraryTitle"] = library.Title ?? string.Empty,
                ["deletedBooksJson"] = JsonSerializer.Serialize(deletedBooks, s_camelCaseJsonOptions),
                ["restoreUrl"] = _urlService.GetAbsoluteUrl(WebRoutes.Books.RESTORE_BOOK, new { bookId = default(Guid) }) ?? string.Empty,
                ["backUrl"] = backUrl,
                ["antiforgeryToken"] = antiforgeryTokens.RequestToken,
                ["strings"] = strings,
                ["stringsJson"] = stringsJson
            }
        };

        Result<ThemePageRenderResultDto> sectionResult = await _themePageRenderer.RenderAsync(pageModel, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
        if (sectionResult.IsFailure)
            return View("/Core/Views/Library/WrittenContentLibrary/BookLibrary/Books/DeletedItems.cshtml",
                new DeletedBooksPageDto(library, deletedBooks),
                new Dictionary<string, object?> { ["Title"] = title });

        return View(
            "/Core/Views/Shared/_ThemedView.cshtml",
            new ThemeViewDto(sectionResult.Value.Content, sectionResult.Value.Script),
            new Dictionary<string, object?> { ["Title"] = title });
    }
}
