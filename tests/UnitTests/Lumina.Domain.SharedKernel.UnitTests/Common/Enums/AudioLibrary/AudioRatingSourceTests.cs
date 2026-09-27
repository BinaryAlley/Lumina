#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.AudioLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AudioRatingSource"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingSourceTests
{
    [Fact]
    public void AudioRatingSource_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        AudioRatingSource[] values = Enum.GetValues<AudioRatingSource>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
