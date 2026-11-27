#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the position reached in a media item during a consumption session.
/// </summary>
public sealed class MediaPosition : ValueObject
{
    /// <summary>
    /// Gets the kind of representation that the position uses.
    /// </summary>
    public MediaPositionKind Kind { get; private set; }

    /// <summary>
    /// Gets the position within the media item, for positions that are expressed as a time offset.
    /// </summary>
    public Optional<TimeSpan> TimeOffset { get; private set; }

    /// <summary>
    /// Gets the opaque locator within the media item, for positions that are not expressed as a time offset, such as a reading location of a book.
    /// </summary>
    public Optional<string> Location { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPosition"/> class.
    /// </summary>
    /// <param name="kind">The kind of representation that the position uses.</param>
    /// <param name="timeOffset">The position within the media item, for positions that are expressed as a time offset.</param>
    /// <param name="location">The opaque locator within the media item, for positions that are not expressed as a time offset.</param>
    private MediaPosition(MediaPositionKind kind, Optional<TimeSpan> timeOffset, Optional<string> location)
    {
        Kind = kind;
        TimeOffset = timeOffset;
        Location = location;
    }

    /// <summary>
    /// Creates a position expressed as a time offset within the media item.
    /// </summary>
    /// <param name="timeOffset">The position within the media item.</param>
    /// <returns>The created <see cref="MediaPosition"/> instance.</returns>
    public static MediaPosition CreateTimeOffset(TimeSpan timeOffset)
    {
        return new MediaPosition(MediaPositionKind.TimeOffset, Optional<TimeSpan>.Some(timeOffset), Optional<string>.None());
    }

    /// <summary>
    /// Creates a position expressed as an opaque locator within the media item.
    /// </summary>
    /// <param name="location">The opaque locator within the media item.</param>
    /// <returns>The created <see cref="MediaPosition"/> instance.</returns>
    public static MediaPosition CreateLocation(string location)
    {
        return new MediaPosition(MediaPositionKind.Location, Optional<TimeSpan>.None(), Optional<string>.Some(location));
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return TimeOffset;
        yield return Location;
    }
}
