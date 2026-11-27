#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Nemera.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Nemera.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="ReadingTocEntryDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingTocEntryDtoMappingTests
{
    private readonly ReadingTocEntryDtoFixture _readingTocEntryDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidEntry_ShouldMapCorrectly()
    {
        // Arrange
        ReadingTocEntryDto entry = _readingTocEntryDtoFixture.Create(children: [_readingTocEntryDtoFixture.Create()]);

        // Act
        ReadingTocEntryResponse result = entry.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entry.Label, result.Label);
        Assert.Equal(entry.LocationRef, result.LocationRef);
        Assert.Equal(entry.Children.Count, result.Children.Count);
        Assert.Equal(entry.Children.Select(child => child.Label), result.Children.Select(child => child.Label));
    }
}
