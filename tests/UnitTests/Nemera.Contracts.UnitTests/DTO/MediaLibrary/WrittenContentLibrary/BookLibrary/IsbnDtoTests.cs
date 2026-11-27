#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="IsbnDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnDtoTests
{
    private readonly IsbnDtoFixture _isbnDtoFixture = new();

    [Fact]
    public void Create_WhenFormatIsIsbn10_ShouldReturnIsbn10Value()
    {
        // Act
        IsbnDto sut = _isbnDtoFixture.Create(format: IsbnFormat.Isbn10);

        // Assert
        Assert.Equal(IsbnFormat.Isbn10, sut.Format);
        Assert.Contains("-", sut.Value, StringComparison.Ordinal);
    }
}
