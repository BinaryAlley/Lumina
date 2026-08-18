#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Services;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="TvShowLibraryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TvShowLibraryServiceTests
{
    [Fact]
    public void TvShows_WhenAccessed_ShouldThrowNotImplementedException()
    {
        // Arrange
        TvShowLibraryService sut = new();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => sut.TvShows);
    }
}
