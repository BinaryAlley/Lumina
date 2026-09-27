#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.AudioLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AudioContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioContentTypeTests
{
    [Fact]
    public void AudioContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        AudioContentType[] values = Enum.GetValues<AudioContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
