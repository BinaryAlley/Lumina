#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackDeliveryShapeExtensions"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryShapeExtensionsTests
{
    [Fact]
    public void IsSeekable_WhenShapeIsByteRangeSeekable_ShouldReturnTrue()
    {
        // Act
        bool isSeekable = PlaybackDeliveryShape.ByteRangeSeekable.IsSeekable();

        // Assert
        Assert.True(isSeekable);
    }

    [Fact]
    public void IsSeekable_WhenShapeIsTimeAddressedSegments_ShouldReturnTrue()
    {
        // Act
        bool isSeekable = PlaybackDeliveryShape.TimeAddressedSegments.IsSeekable();

        // Assert
        Assert.True(isSeekable);
    }

    [Fact]
    public void IsSeekable_WhenShapeIsUndefined_ShouldReturnFalse()
    {
        // Arrange
        PlaybackDeliveryShape undefinedShape = (PlaybackDeliveryShape)99;

        // Act
        bool isSeekable = undefinedShape.IsSeekable();

        // Assert
        Assert.False(isSeekable);
    }
}
