#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingManifestRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestRequestMappingTests
{
    private readonly GetReadingManifestRequestFixture _requestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetReadingManifestRequest request = _requestFixture.Create();

        // Act
        GetReadingManifestQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.BookId, result.BookId);
    }
}
