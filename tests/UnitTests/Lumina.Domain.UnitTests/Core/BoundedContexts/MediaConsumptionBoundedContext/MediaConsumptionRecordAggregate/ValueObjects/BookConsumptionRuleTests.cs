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
/// Contains unit tests for the <see cref="BookConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookConsumptionRuleTests
{
    private readonly BookConsumptionRuleFixture _bookConsumptionRuleFixture = new();
    private readonly BookConsumptionRule _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookConsumptionRuleTests"/> class.
    /// </summary>
    public BookConsumptionRuleTests()
    {
        _sut = _bookConsumptionRuleFixture.Create();
    }

    [Fact]
    public void MediaKind_WhenRead_ShouldBeBook()
    {
        // Act
        MediaKind result = _sut.MediaKind;

        // Assert
        Assert.Equal(MediaKind.Book, result);
    }

    [Fact]
    public void IsSupportedByCurrentCatalogues_WhenRead_ShouldBeTrue()
    {
        // Act
        bool result = _sut.IsSupportedByCurrentCatalogues;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PositionKind_WhenRead_ShouldBeLocation()
    {
        // Act
        Optional<MediaPositionKind> result = _sut.PositionKind;

        // Assert
        Assert.True(result.HasValue);
        Assert.Equal(MediaPositionKind.Location, result.Value);
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
    public void MinimumConsecutivePages_WhenRead_ShouldBeFive()
    {
        // Act
        int result = _sut.MinimumConsecutivePages;

        // Assert
        Assert.Equal(5, result);
    }
}
