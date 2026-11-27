#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.Libraries;
using Nemera.Presentation.Web.Common.DTO.Themes;
using Nemera.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Common.Http;
using Nemera.Presentation.Web.Common.Primitives;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Common.Services;
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeletedBooks;
using Nemera.Presentation.Web.Core.Themes;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Themes;
using Nemera.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeletedBooks;

/// <summary>
/// Contains unit tests for the <see cref="DeletedBooksViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedBooksViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IAntiforgery _mockAntiforgery;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly DeletedBooksViewEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly DeletedBookDtoFixture _deletedBookDtoFixture = new();
    private readonly GetBooksViewRequestFixture _getBooksViewRequestFixture = new();
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedBooksViewEndpointTests"/> class.
    /// </summary>
    public DeletedBooksViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockUrlService = Substitute.For<IUrlService>();
        _mockThemePageRenderer = Substitute.For<ThemePageRenderer>(new ThemeService(_mockApiHttpClient), new ThemeTemplateEngine());
        _mockAntiforgery = Substitute.For<IAntiforgery>();
        _mockAntiforgery.GetAndStoreTokens(Arg.Any<HttpContext>())
            .Returns(new AntiforgeryTokenSet("request-token", "cookie-token", "__RequestVerificationToken", "RequestVerificationToken"));
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(
            callInfo.Arg<string>(),
            callInfo.Arg<string>() == "Title" ? "Deleted items" : $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[]
        {
            new("Title", "Deleted items"),
            new("NoDeletedItems", "There are no deleted items in this library.")
        });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<DeletedBooksViewEndpoint>(_mockApiHttpClient, _mockUrlService, _mockThemePageRenderer, _mockAntiforgery, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/library/written-content-library/books-library/books/deleted-items";

        StubUrls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingSucceeds_ShouldRenderThemedViewWithDocumentedPageData()
    {
        // Arrange
        LibraryDto library = _libraryDtoFixture.Create(title: "Books");
        DeletedBookDto[] deletedBooks = [.. _deletedBookDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(library);
        _mockApiHttpClient.GetAsync<DeletedBookDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedBooks);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create(content: "<section>deleted</section>", script: "<script>deleted</script>")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getBooksViewRequestFixture.Create(libraryId: library.Id), CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Shared/_ThemedView.cshtml", GetRenderedViewName(result));
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "library/written-content-library/book-library/books/deleted-items" &&
                page.Title == "Deleted items" &&
                string.Equals(page.PageData["libraryId"] as string, library.Id!.Value.ToString(), StringComparison.Ordinal) &&
                string.Equals(page.PageData["libraryTitle"] as string, "Books", StringComparison.Ordinal) &&
                string.Equals(page.PageData["restoreUrl"] as string, "http://localhost/en-us/library/written-content-library/books-library/books/00000000-0000-0000-0000-000000000000/api-restore-book", StringComparison.Ordinal) &&
                string.Equals(page.PageData["backUrl"] as string, $"http://localhost/en-us/library/written-content-library/books-library/books?libraryId={library.Id}", StringComparison.Ordinal) &&
                string.Equals(page.PageData["antiforgeryToken"] as string, "request-token", StringComparison.Ordinal) &&
                page.PageData.ContainsKey("deletedBooksJson") &&
                page.PageData.ContainsKey("strings") &&
                page.PageData.ContainsKey("stringsJson")),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdIsNull_ShouldRedirectToHome()
    {
        // Arrange
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED).Returns("http://localhost/en-us");

        // Act
        IResult result = await _sut.ExecuteAsync(_getBooksViewRequestFixture.Create(libraryId: null), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("http://localhost/en-us", redirectResult.Url);
        await _mockApiHttpClient.DidNotReceive().GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemePageRenderer.DidNotReceive().RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToTheRazorViewWithTheDeletedBooks()
    {
        // Arrange
        LibraryDto library = _libraryDtoFixture.Create(title: "Books");
        DeletedBookDto[] deletedBooks = [.. _deletedBookDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(library);
        _mockApiHttpClient.GetAsync<DeletedBookDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(deletedBooks);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getBooksViewRequestFixture.Create(libraryId: library.Id), CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Library/WrittenContentLibrary/BookLibrary/Books/DeletedItems.cshtml", GetRenderedViewName(result));
        DeletedBooksPageDto model = Assert.IsType<DeletedBooksPageDto>(GetRenderedModel(result));
        Assert.Equal(library.Id, model.Library.Id);
        Assert.Equal(deletedBooks, model.DeletedBooks);
    }

    /// <summary>
    /// Stubs the absolute URLs that the endpoint resolves for the deleted items page.
    /// </summary>
    private void StubUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Books.INDEX).Returns("http://localhost/en-us/library/written-content-library/books-library/books");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Books.RESTORE_BOOK, Arg.Any<object?>())
            .Returns("http://localhost/en-us/library/written-content-library/books-library/books/00000000-0000-0000-0000-000000000000/api-restore-book");
    }

    /// <summary>
    /// Reads the private view name of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The name or path of the rendered view.</returns>
    private static string GetRenderedViewName(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? viewNameField = typeof(RazorViewResult).GetField("_viewName", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewNameField);
        return (string)viewNameField.GetValue(viewResult)!;
    }

    /// <summary>
    /// Reads the private model of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The model passed to the rendered view.</returns>
    private static object? GetRenderedModel(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? modelField = typeof(RazorViewResult).GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(modelField);
        return modelField.GetValue(viewResult);
    }
}
