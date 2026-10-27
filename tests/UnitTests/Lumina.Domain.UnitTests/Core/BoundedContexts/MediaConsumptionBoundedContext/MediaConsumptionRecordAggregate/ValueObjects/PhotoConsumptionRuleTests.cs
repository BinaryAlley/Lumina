#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="PhotoConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PhotoConsumptionRuleTests
{
    private readonly PhotoConsumptionRuleFixture _photoConsumptionRuleFixture = new();
    private readonly PhotoConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotoConsumptionRuleTests"/> class.
    /// </summary>
    public PhotoConsumptionRuleTests()
    {
        _sut = _photoConsumptionRuleFixture.Create();
    }

    [Fact]
    public void MediaKind_WhenRead_ShouldBePhoto()
    {
        // Act
        MediaKind result = _sut.MediaKind;

        // Assert
        Assert.Equal(MediaKind.Photo, result);
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
    public void PositionKind_WhenRead_ShouldHaveNoValue()
    {
        // Act
        Optional<MediaPositionKind> result = _sut.PositionKind;

        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public void IsOneShot_WhenRead_ShouldBeTrue()
    {
        // Act
        bool result = _sut.IsOneShot;

        // Assert
        Assert.True(result);
    }
}
