namespace Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;

/// <summary>
/// Enumeration for the kinds of position that a media consumption session can report.
/// </summary>
public enum MediaPositionKind
{
    /// <summary>
    /// The position is a time offset within the media item.
    /// </summary>
    TimeOffset,

    /// <summary>
    /// The position is an opaque locator within the media item, such as a reading location of a book.
    /// </summary>
    Location
}
