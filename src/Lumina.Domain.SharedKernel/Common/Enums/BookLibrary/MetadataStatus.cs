namespace Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;

/// <summary>
/// Enumeration for the status of the metadata enrichment of a book.
/// </summary>
public enum MetadataStatus
{
    /// <summary>
    /// The metadata of the book has not been enriched yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The metadata of the book has been successfully enriched.
    /// </summary>
    Enriched,

    /// <summary>
    /// The metadata enrichment of the book failed.
    /// </summary>
    Failed
}
