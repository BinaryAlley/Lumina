#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;

/// <summary>
/// Contains unit tests for the <see cref="GetBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookEndpointTests
{
    private readonly GetBookEndpoint _sut = Factory.Create<GetBookEndpoint>();
    private readonly GetBookRequestFixture _getBookRequestFixture = new();

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResult()
    {
        // Arrange
        GetBookRequest request = _getBookRequestFixture.Create(id: Guid.NewGuid().ToString());
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.IsType<Ok>(result);
    }
}
