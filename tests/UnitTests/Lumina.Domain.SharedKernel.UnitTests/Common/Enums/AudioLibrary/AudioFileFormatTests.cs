#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.AudioLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AudioFileFormat"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioFileFormatTests
{
    [Fact]
    public void AudioFileFormat_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        AudioFileFormat[] values = Enum.GetValues<AudioFileFormat>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void None_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)AudioFileFormat.None;

        // Assert
        Assert.Equal(0, value);
    }
}
