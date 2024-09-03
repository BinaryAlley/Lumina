#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.Books;
using Lumina.Presentation.Web.Common.Models.Contributors;
using Lumina.Presentation.Web.Common.Enums;
#endregion

namespace Lumina.Presentation.Web.Common.Contracts.Books;

/// <summary>
/// Represents a request to add a book.
/// </summary>
public class AddBookRequest
{
    #region ==================================================================== PROPERTIES =================================================================================
    public WrittenContentMetadataDto? Metadata { get; set; }
    public BookFormat? Format { get; set; }
    public string? Edition { get; set; }
    public int? VolumeNumber { get; set; }
    public BookSeriesDto? Series { get; set; }
    public string? ASIN { get; set; }
    public string? GoodreadsId { get; set; }
    public string? LCCN { get; set; }
    public string? OCLCNumber { get; set; }
    public string? OpenLibraryId { get; set; }
    public string? LibraryThingId { get; set; }
    public string? GoogleBooksId { get; set; }
    public string? BarnesAndNobleId { get; set; }
    public string? AppleBooksId { get; set; }
    public List<IsbnDto>? ISBNs { get; set; }
    public List<MediaContributorDto>? Contributors { get; set; }
    public List<BookRatingDto>? Ratings { get; set; }
    #endregion
}