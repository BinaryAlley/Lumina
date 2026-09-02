#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingResourceRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceRequestMappingTests
{
    private readonly GetReadingResourceRequestFixture _requestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetReadingResourceRequest request = _requestFixture.Create();

        // Act
        GetReadingResourceQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.BookId, result.BookId);
        Assert.Equal(request.ResourceKey, result.ResourceKey);
    }
}
