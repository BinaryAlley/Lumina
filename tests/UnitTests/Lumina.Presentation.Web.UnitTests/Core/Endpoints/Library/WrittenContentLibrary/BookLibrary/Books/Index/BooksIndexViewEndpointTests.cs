#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;

/// <summary>
/// Contains unit tests for the <see cref="BooksIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksIndexViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IUrlService _mockUrlService;
    private readonly ThemePageRenderer _mockThemePageRenderer;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly BooksIndexViewEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly GetBooksViewRequestFixture _getBooksViewRequestFixture = new();
    private readonly ThemePageRenderResultDtoFixture _themePageRenderResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksIndexViewEndpointTests"/> class.
    /// </summary>
    public BooksIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockUrlService = Substitute.For<IUrlService>();
        _mockThemePageRenderer = Substitute.For<ThemePageRenderer>(new ThemeService(_mockApiHttpClient), new ThemeTemplateEngine());
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(new LocalizedString[] { new("Search", "Search") });
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _sut = Factory.Create<BooksIndexViewEndpoint>(_mockApiHttpClient, _mockUrlService, _mockThemePageRenderer, _mockStringLocalizerFactory);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/library/written-content-library/books-library/books";
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
    public async Task ExecuteAsync_WhenLibraryIdProvided_ShouldRenderThemedPageThroughTheSharedView()
    {
        // Arrange
        LibraryDto expectedLibrary = _libraryDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedLibrary);
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Books.GET_LIBRARY_ITEMS).Returns("http://localhost/en-us/library/written-content-library/books-library/books/api-get-library-items");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Settings.GET_USER_SETTINGS).Returns("http://localhost/en-us/tools/settings/api-get-user-settings");
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themePageRenderResultDtoFixture.Create(content: "<section>content</section>", script: "<script>script</script>")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getBooksViewRequestFixture.Create(libraryId: expectedLibrary.Id), CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(
            Arg.Is<ThemePageDto>(page =>
                page.PageKey == "library/written-content-library/book-library/books/index" &&
                page.Title == expectedLibrary.Title &&
                page.PageData.ContainsKey("libraryId") &&
                page.PageData.ContainsKey("settingsUrl") &&
                page.PageData.ContainsKey("strings") &&
                string.Equals(
                    page.PageData["itemsUrl"] as string,
                    "http://localhost/en-us/library/written-content-library/books-library/books/api-get-library-items",
                    StringComparison.Ordinal)),
            Arg.Is<string?>(requestedThemeId => requestedThemeId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeRenderingFails_ShouldFallBackToRazorView()
    {
        // Arrange
        LibraryDto expectedLibrary = _libraryDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedLibrary);
        _mockThemePageRenderer.RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result<ThemePageRenderResultDto>.Failure(Error.Failure("Theme.Render", "The page could not be rendered.")));

        // Act
        IResult result = await _sut.ExecuteAsync(_getBooksViewRequestFixture.Create(libraryId: expectedLibrary.Id), CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        await _mockThemePageRenderer.Received(1).RenderAsync(Arg.Any<ThemePageDto>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
