namespace Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;

/// <summary>
/// Enumeration for the status of the artwork enrichment of a book artwork.
/// </summary>
public enum ArtworkStatus
{
    /// <summary>
    /// The artwork of the book has not been resolved yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The artwork of the book has been successfully resolved.
    /// </summary>
    Enriched,

    /// <summary>
    /// The artwork resolution of the book failed.
    /// </summary>
    Failed
}
