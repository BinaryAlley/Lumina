#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="ReadingDocumentDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingDocumentDtoMappingTests
{
    private readonly ReadingDocumentDtoFixture _readingDocumentDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidDocument_ShouldMapCorrectly()
    {
        // Arrange
        ReadingDocumentDto document = _readingDocumentDtoFixture.Create();

        // Act
        ReadingManifestResponse result = document.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(document.Title, result.Title);
        Assert.Equal(document.Author, result.Author);
        Assert.Equal(document.CoverResourceKey, result.CoverResourceKey);
        Assert.Equal(document.TableOfContents.Count, result.TableOfContents.Count);
        Assert.Equal(document.Spine.Select(item => item.LocationRef), result.Spine.Select(item => item.LocationRef));
        Assert.Equal(document.Spine.Select(item => item.Title), result.Spine.Select(item => item.Title));
        Assert.Equal(document.Resources.Keys, result.ResourceKeys);
        Assert.Equal(document.HasTextContent, result.HasTextContent);
    }
}
