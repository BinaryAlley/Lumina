namespace Lumina.Domain.Common.Enums;

/// <summary>
/// Enumeration for various book formats.
/// </summary>
public enum BookFormat
{
    /// <summary>
    /// A book bound with rigid protective covers (typically cardboard covered with cloth, heavy paper, or leather).
    /// </summary>
    Hardcover,

    /// <summary>
    /// A book with a flexible paper cover, typically less durable than a hardcover.
    /// </summary>
    Paperback,

    /// <summary>
    /// A small, usually non-illustrated, and inexpensive paperback book.
    /// </summary>
    MassMarketPaperback,

    /// <summary>
    /// A larger, often higher-quality paperback book, typically used for literary fiction and nonfiction.
    /// </summary>
    TradePaperback,

    /// <summary>
    /// A digital version of a book that can be read on electronic devices.
    /// </summary>
    eBook,

    /// <summary>
    /// A recorded audio version of a book, either on physical media or as a digital file.
    /// </summary>
    Audiobook,

    /// <summary>
    /// A book printed with larger text for easier reading, often used by those with visual impairments.
    /// </summary>
    LargePrint,

    /// <summary>
    /// A sturdy book with thick cardboard pages, typically designed for young children.
    /// </summary>
    BoardBook,

    /// <summary>
    /// A book bound with a spiral wire or plastic coil, allowing it to lay flat when open.
    /// </summary>
    SpiralBound,

    /// <summary>
    /// A reinforced binding designed for frequent use, often used in schools and public libraries.
    /// </summary>
    LibraryBinding,

    /// <summary>
    /// A book bound in leather, often used for luxury or collector's editions.
    /// </summary>
    LeatherBound,

    /// <summary>
    /// A book with three-dimensional paper engineering, creating pop-up or movable parts.
    /// </summary>
    PopupBook
}