#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Http;
using Nemera.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;

/// <summary>
/// Contains unit tests for the <see cref="ReadViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadViewEndpointTests
{
    private readonly ReadViewEndpoint _sut = Factory.Create<ReadViewEndpoint>();
    private readonly ReadBookViewRequestFixture _readBookViewRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadViewEndpointTests"/> class.
    /// </summary>
    public ReadViewEndpointTests()
    {
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/library/written-content-library/books-library/books/00000000-0000-0000-0000-000000000000/read";
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnTheReadRazorView()
    {
        // Arrange
        ReadBookViewRequest request = _readBookViewRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
    }
}
