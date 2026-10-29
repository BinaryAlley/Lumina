#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaPosition"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionTests
{
    private readonly MediaPositionFixture _mediaPositionFixture = new();

    [Fact]
    public void CreateTimeOffset_WhenCalled_ShouldSetTimeOffsetKindAndTheOffset()
    {
        // Arrange
        TimeSpan timeOffset = TimeSpan.FromMinutes(42);

        // Act
        MediaPosition position = MediaPosition.CreateTimeOffset(timeOffset);

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, position.Kind);
        Assert.True(position.TimeOffset.HasValue);
        Assert.Equal(timeOffset, position.TimeOffset.Value);
        Assert.False(position.Location.HasValue);
    }

    [Fact]
    public void CreateLocation_WhenCalled_ShouldSetLocationKindAndTheLocation()
    {
        // Arrange
        string location = "chapter-7/page-12";

        // Act
        MediaPosition position = MediaPosition.CreateLocation(location);

        // Assert
        Assert.Equal(MediaPositionKind.Location, position.Kind);
        Assert.True(position.Location.HasValue);
        Assert.Equal(location, position.Location.Value);
        Assert.False(position.TimeOffset.HasValue);
    }

    [Fact]
    public void CreateTimeOffset_WhenOffsetIsNegative_ShouldStoreTheOffsetAsGiven()
    {
        // Arrange
        TimeSpan negativeTimeOffset = TimeSpan.FromMinutes(-1);

        // Act
        MediaPosition position = MediaPosition.CreateTimeOffset(negativeTimeOffset);

        // Assert
        // the value object does not validate the sign of the offset, the reported value is stored as given
        Assert.Equal(TimeSpan.FromMinutes(-1), position.TimeOffset.Value);
    }

    [Fact]
    public void CreateLocation_WhenLocationIsEmpty_ShouldStoreTheEmptyLocation()
    {
        // Act
        MediaPosition position = MediaPosition.CreateLocation(string.Empty);

        // Assert
        // the value object does not validate the content of the location, the reported value is stored as given
        Assert.True(position.Location.HasValue);
        Assert.Equal(string.Empty, position.Location.Value);
    }

    [Fact]
    public void Equals_WhenKindAndValueAreTheSame_ShouldReturnTrue()
    {
        // Arrange
        MediaPosition firstPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromMinutes(10));
        MediaPosition secondPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromMinutes(10));

        // Act
        bool result = firstPosition.Equals(secondPosition);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WhenTimeOffsetDiffers_ShouldReturnFalse()
    {
        // Arrange
        MediaPosition firstPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromMinutes(10));
        MediaPosition secondPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromMinutes(11));

        // Act
        bool result = firstPosition.Equals(secondPosition);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenLocationDiffers_ShouldReturnFalse()
    {
        // Arrange
        MediaPosition firstPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location, location: "chapter-1");
        MediaPosition secondPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location, location: "chapter-2");

        // Act
        bool result = firstPosition.Equals(secondPosition);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenKindDiffers_ShouldReturnFalse()
    {
        // Arrange
        MediaPosition firstPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromMinutes(10));
        MediaPosition secondPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location, location: "chapter-1");

        // Act
        bool result = firstPosition.Equals(secondPosition);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WhenKindAndValueAreTheSame_ShouldReturnSameHashCode()
    {
        // Arrange
        MediaPosition firstPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location, location: "chapter-3");
        MediaPosition secondPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location, location: "chapter-3");

        // Act
        int firstHashCode = firstPosition.GetHashCode();
        int secondHashCode = secondPosition.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldYieldKindThenTimeOffsetThenLocation()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        object?[] components = [.. position.GetEqualityComponents()];

        // Assert
        Assert.Equal(3, components.Length);
        Assert.Equal(MediaPositionKind.TimeOffset, Assert.IsType<MediaPositionKind>(components[0]));
        Assert.Equal(position.TimeOffset, Assert.IsType<Optional<TimeSpan>>(components[1]));
        Assert.Equal(position.Location, Assert.IsType<Optional<string>>(components[2]));
    }
}
