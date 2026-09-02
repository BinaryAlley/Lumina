#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionRequestMappingTests
{
    private readonly GetReadingSectionRequestFixture _requestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetReadingSectionRequest request = _requestFixture.Create();

        // Act
        GetReadingSectionQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.BookId, result.BookId);
        Assert.Equal(request.LocationRef, result.LocationRef);
    }
}
