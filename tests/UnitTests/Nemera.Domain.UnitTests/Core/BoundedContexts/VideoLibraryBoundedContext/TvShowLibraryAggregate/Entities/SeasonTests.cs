#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Season"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SeasonTests
{
    private readonly SeasonIdFixture _seasonIdFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetId()
    {
        // Arrange
        SeasonId id = _seasonIdFixture.Create();

        // Act
        Season season = new(id);

        // Assert
        Assert.Equal(id, season.Id);
    }
}
