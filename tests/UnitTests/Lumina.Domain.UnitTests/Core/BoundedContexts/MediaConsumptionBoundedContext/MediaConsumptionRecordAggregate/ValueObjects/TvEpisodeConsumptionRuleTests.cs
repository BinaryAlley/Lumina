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
/// Contains unit tests for the <see cref="TvEpisodeConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TvEpisodeConsumptionRuleTests
{
    private readonly TvEpisodeConsumptionRuleFixture _tvEpisodeConsumptionRuleFixture = new();
    private readonly TvEpisodeConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="TvEpisodeConsumptionRuleTests"/> class.
    /// </summary>
    public TvEpisodeConsumptionRuleTests()
    {
        _sut = _tvEpisodeConsumptionRuleFixture.Create();
    }

    [Fact]
    public void MediaKind_WhenRead_ShouldBeTvEpisode()
    {
        // Act
        MediaKind result = _sut.MediaKind;

        // Assert
        Assert.Equal(MediaKind.TvEpisode, result);
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
    public void MinimumCountedDuration_WhenRead_ShouldBeThreeMinutes()
    {
        // Act
        TimeSpan result = _sut.MinimumCountedDuration;

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(3), result);
    }
}
