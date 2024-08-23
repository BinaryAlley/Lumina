namespace Lumina.Domain.Common.Enums;

/// <summary>
/// Enumeration for the different ISBN (International Standard Book Number) formats.
/// </summary>
public enum IsbnFormat
{
    /// <summary>
    /// The 10-digit ISBN format used for books published before 2007.
    /// Example: 0-306-40615-2
    /// </summary>
    Isbn10,

    /// <summary>
    /// The 13-digit ISBN format introduced in 2007, now the standard for all new books.
    /// Example: 978-0-306-40615-7
    /// </summary>
    Isbn13
}