#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private readonly BooksIndexViewEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksIndexViewEndpointTests"/> class.
    /// </summary>
    public BooksIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockUrlService = Substitute.For<IUrlService>();
        _sut = Factory.Create<BooksIndexViewEndpoint>(_mockApiHttpClient, _mockUrlService);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/library/written-content-library/books-library/books";
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdIsNull_ShouldRedirectToHome()
    {
        // Arrange
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED).Returns("http://localhost/en-us");

        // Act
        IResult result = await _sut.ExecuteAsync(new GetBooksViewRequest(null), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("http://localhost/en-us", redirectResult.Url);
        await _mockApiHttpClient.DidNotReceive().GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdProvided_ShouldReturnRazorViewResultWithLibraryModel()
    {
        // Arrange
        LibraryDto expectedLibrary = _libraryDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedLibrary);

        // Act
        IResult result = await _sut.ExecuteAsync(new GetBooksViewRequest(expectedLibrary.Id), CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
        string expectedEndpoint = ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", expectedLibrary.Id!.Value.ToString());
        await _mockApiHttpClient.Received(1).GetAsync<LibraryDto>(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
