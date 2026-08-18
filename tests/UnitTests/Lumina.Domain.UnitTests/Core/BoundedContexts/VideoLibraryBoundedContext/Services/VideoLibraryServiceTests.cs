#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.Services;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.Services;

/// <summary>
/// Contains unit tests for the <see cref="VideoLibraryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoLibraryServiceTests
{
    [Fact]
    public void Constructor_WhenCalled_ShouldSetInjectedServices()
    {
        // Arrange
        IMovieLibraryService movieLibraryService = Substitute.For<IMovieLibraryService>();
        ITvShowLibraryService tvShowLibraryService = Substitute.For<ITvShowLibraryService>();

        // Act
        VideoLibraryService sut = new(movieLibraryService, tvShowLibraryService);

        // Assert
        Assert.Same(movieLibraryService, sut.MovieLibraryService);
        Assert.Same(tvShowLibraryService, sut.TvShowLibraryService);
    }
}
