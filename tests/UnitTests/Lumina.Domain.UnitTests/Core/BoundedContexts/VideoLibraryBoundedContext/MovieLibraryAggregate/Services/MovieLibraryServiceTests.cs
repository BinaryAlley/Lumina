#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.Services;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="MovieLibraryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovieLibraryServiceTests
{
    [Fact]
    public void Movies_WhenAccessed_ShouldThrowNotImplementedException()
    {
        // Arrange
        MovieLibraryService sut = new();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => sut.Movies);
    }
}
