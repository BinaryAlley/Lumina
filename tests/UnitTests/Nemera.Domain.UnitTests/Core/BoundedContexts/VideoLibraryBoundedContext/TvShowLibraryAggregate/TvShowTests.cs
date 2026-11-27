#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="TvShow"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TvShowTests
{
    private readonly TvShowIdFixture _tvShowIdFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetId()
    {
        // Arrange
        TvShowId id = _tvShowIdFixture.Create();

        // Act
        TvShow tvShow = new(id);

        // Assert
        Assert.Equal(id, tvShow.Id);
    }
}
