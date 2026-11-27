#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.VideoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.VideoLibrary;

/// <summary>
/// Contains unit tests for the <see cref="VideoRatingSource"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoRatingSourceTests
{
    [Fact]
    public void VideoRatingSource_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VideoRatingSource[] values = Enum.GetValues<VideoRatingSource>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
