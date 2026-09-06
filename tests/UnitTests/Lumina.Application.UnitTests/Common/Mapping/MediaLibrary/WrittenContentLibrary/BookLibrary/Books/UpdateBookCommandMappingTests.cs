#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCommandMappingTests
{
    private readonly UpdateBookCommandFixture _commandFixture = new();

    [Fact]
    public void ToBookMetadataDto_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        UpdateBookCommand command = _commandFixture.Create();

        // Act
        BookMetadataDto result = command.ToBookMetadataDto();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Metadata!.Title, result.Title);
        Assert.Equal(command.Metadata.OriginalTitle, result.OriginalTitle);
        Assert.Equal(command.Metadata.Description, result.Description);
        Assert.Equal(command.Metadata.ReleaseInfo, result.ReleaseInfo);
        Assert.Equal(command.Metadata.Genres, result.Genres);
        Assert.Equal(command.Metadata.Tags, result.Tags);
        Assert.Equal(command.Metadata.Language, result.Language);
        Assert.Equal(command.Metadata.OriginalLanguage, result.OriginalLanguage);
        Assert.Equal(command.Metadata.Publisher, result.Publisher);
        Assert.Equal(command.Metadata.PageCount, result.PageCount);
        Assert.Equal(command.Format, result.Format);
        Assert.Equal(command.Edition, result.Edition);
        Assert.Equal(command.VolumeNumber, result.VolumeNumber);
        Assert.Equal(command.Series, result.Series);
        Assert.Equal(command.ASIN, result.ASIN);
        Assert.Equal(command.GoodreadsId, result.GoodreadsId);
        Assert.Equal(command.LCCN, result.LCCN);
        Assert.Equal(command.OCLCNumber, result.OCLCNumber);
        Assert.Equal(command.OpenLibraryId, result.OpenLibraryId);
        Assert.Equal(command.LibraryThingId, result.LibraryThingId);
        Assert.Equal(command.GoogleBooksId, result.GoogleBooksId);
        Assert.Equal(command.BarnesAndNobleId, result.BarnesAndNobleId);
        Assert.Equal(command.AppleBooksId, result.AppleBooksId);
        Assert.Equal(command.ISBNs, result.Isbns);
        Assert.Equal(command.Contributors, result.Contributors);
        Assert.Equal(command.Ratings, result.Ratings);
        Assert.Null(result.CoverImagePath);
    }
}
