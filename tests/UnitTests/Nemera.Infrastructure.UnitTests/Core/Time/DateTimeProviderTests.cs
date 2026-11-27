#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Time;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Time;

/// <summary>
/// Contains unit tests for the <see cref="DateTimeProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DateTimeProviderTests
{
    private readonly DateTimeProvider _sut = new();

    [Fact]
    public void UtcNow_WhenCalled_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;

        // Act
        DateTime result = _sut.UtcNow;

        // Assert
        DateTime after = DateTime.UtcNow;
        Assert.True(result >= before && result <= after);
    }

    [Fact]
    public void UtcNow_WhenCalled_ShouldReturnUtcKind()
    {
        // Act
        DateTime result = _sut.UtcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }
}
