#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.VideoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.VideoLibrary;

/// <summary>
/// Contains unit tests for the <see cref="VideoContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoContentTypeTests
{
    [Fact]
    public void VideoContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VideoContentType[] values = Enum.GetValues<VideoContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
