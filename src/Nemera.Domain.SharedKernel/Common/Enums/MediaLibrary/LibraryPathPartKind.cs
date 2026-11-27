namespace Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the kinds of parts that make up a media library path template.
/// </summary>
public enum LibraryPathPartKind
{
    /// <summary>
    /// A free text part, used for arbitrary on-disk text and for the literal glue between value parts (e.g. " - ").
    /// </summary>
    Literal,

    /// <summary>
    /// The path separator between two directories of the library structure.
    /// </summary>
    Separator,

    /// <summary>
    /// The name of the artist the release belongs to.
    /// </summary>
    Artist,

    /// <summary>
    /// The type of the release (e.g. Album, Single, Compilation).
    /// </summary>
    ReleaseType,

    /// <summary>
    /// The year the release was published.
    /// </summary>
    ReleaseYear,

    /// <summary>
    /// The title of the release.
    /// </summary>
    ReleaseName,

    /// <summary>
    /// The number of the track on its disc.
    /// </summary>
    TrackNumber,

    /// <summary>
    /// The title of the track.
    /// </summary>
    TrackName,

    /// <summary>
    /// The file name extension of the track, without the leading dot.
    /// </summary>
    Extension,

    /// <summary>
    /// The number of the disc the track belongs to.
    /// </summary>
    DiscNumber,

    /// <summary>
    /// The name of the author of a book.
    /// </summary>
    Author,

    /// <summary>
    /// The title of a book.
    /// </summary>
    Title,

    /// <summary>
    /// The name of the series a book belongs to.
    /// </summary>
    Series,

    /// <summary>
    /// The number of the book within its series.
    /// </summary>
    SeriesNumber,

    /// <summary>
    /// The identifier Calibre assigns to a book on disk.
    /// </summary>
    BookId
}
