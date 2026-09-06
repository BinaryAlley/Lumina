#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetBookRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookRequestMappingTests
{
    private readonly GetBookRequestFixture _requestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingRequestWithValidId_ShouldMapIdCorrectly()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        GetBookRequest request = _requestFixture.Create(id: bookId.ToString());

        // Act
        GetBookQuery result = request.ToQuery();

        // Assert
        Assert.Equal(bookId, result.Id);
    }

    [Fact]
    public void ToQuery_WhenMappingRequestWithInvalidId_ShouldMapEmptyGuid()
    {
        // Arrange
        GetBookRequest request = _requestFixture.Create(id: "not-a-guid");

        // Act
        GetBookQuery result = request.ToQuery();

        // Assert
        Assert.Equal(Guid.Empty, result.Id);
    }
}
