#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingResourceDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingResourceDataDtoTests
{
    private readonly ReadingResourceDataDtoFixture _readingResourceDataDtoFixture = new();

    [Fact]
    public void Create_WhenProvidedData_ShouldPreserveIt()
    {
        // Arrange
        byte[] data = Guid.NewGuid().ToByteArray();

        // Act
        ReadingResourceDataDto sut = _readingResourceDataDtoFixture.Create(data: data);

        // Assert
        Assert.Equal(data, sut.Data);
    }
}
