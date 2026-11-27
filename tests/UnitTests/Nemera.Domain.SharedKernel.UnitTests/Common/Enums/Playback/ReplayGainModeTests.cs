#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playback;

/// <summary>
/// Contains unit tests for the <see cref="ReplayGainMode"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReplayGainModeTests
{
    [Fact]
    public void ReplayGainMode_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ReplayGainMode[] values = Enum.GetValues<ReplayGainMode>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void ReplayGainMode_WhenEnumeratingNames_ShouldHaveTheThreeDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<ReplayGainMode>();

        // Assert
        string[] expectedNames =
        [
            nameof(ReplayGainMode.Off),
            nameof(ReplayGainMode.Track),
            nameof(ReplayGainMode.Album)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void ReplayGainMode_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        ReplayGainMode[] values = Enum.GetValues<ReplayGainMode>();

        // Assert
        ReplayGainMode[] expectedValues =
        [
            ReplayGainMode.Off,
            ReplayGainMode.Track,
            ReplayGainMode.Album
        ];
        int[] expectedUnderlyingValues = [0, 1, 2];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void ReplayGainMode_WhenDefaulted_ShouldBeOff()
    {
        // Act
        ReplayGainMode replayGainMode = default;

        // Assert
        Assert.Equal(ReplayGainMode.Off, replayGainMode);
    }
}
