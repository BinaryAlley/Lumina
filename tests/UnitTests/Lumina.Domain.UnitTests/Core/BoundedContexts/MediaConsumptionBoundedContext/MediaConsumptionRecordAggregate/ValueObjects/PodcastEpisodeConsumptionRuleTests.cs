#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="PodcastEpisodeConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PodcastEpisodeConsumptionRuleTests
{
    private readonly PodcastEpisodeConsumptionRuleFixture _podcastEpisodeConsumptionRuleFixture = new();
    private readonly PodcastEpisodeConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastEpisodeConsumptionRuleTests"/> class.
    /// </summary>
    public PodcastEpisodeConsumptionRuleTests()
    {
        _sut = _podcastEpisodeConsumptionRuleFixture.Create();
    }

    [Fact]
    public void MediaKind_WhenRead_ShouldBePodcastEpisode()
    {
        // Act
        MediaKind result = _sut.MediaKind;

        // Assert
        Assert.Equal(MediaKind.PodcastEpisode, result);
    }

    [Fact]
    public void IsSupportedByCurrentCatalogues_WhenRead_ShouldBeFalse()
    {
        // Act
        bool result = _sut.IsSupportedByCurrentCatalogues;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PositionKind_WhenRead_ShouldBeTimeOffset()
    {
        // Act
        Optional<MediaPositionKind> result = _sut.PositionKind;

        // Assert
        Assert.True(result.HasValue);
        Assert.Equal(MediaPositionKind.TimeOffset, result.Value);
    }

    [Fact]
    public void IsOneShot_WhenRead_ShouldBeFalse()
    {
        // Act
        bool result = _sut.IsOneShot;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MinimumCountedDuration_WhenRead_ShouldBeThirtySeconds()
    {
        // Act
        TimeSpan result = _sut.MinimumCountedDuration;

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(30), result);
    }
}
