#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.ValueObjects.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="Mood"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MoodTests
{
    private readonly MoodFixture _moodFixture = new();

    [Fact]
    public void Create_WhenCalledWithUntrimmedName_ShouldTrimTheName()
    {
        // Act
        Result<Mood> result = Mood.Create("  calm  ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("calm", result.Value.Name);
    }

    [Theory]
    [InlineData(null)] // null mood name
    [InlineData("")] // empty mood name
    [InlineData("   ")] // whitespace mood name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<Mood> result = Mood.Create(name!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.MoodNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnName()
    {
        // Arrange
        Mood mood = _moodFixture.Create(name: "energetic");

        // Act
        string result = mood.ToString();

        // Assert
        Assert.Equal("energetic", result);
    }

    [Fact]
    public void Equals_WithSameName_ShouldReturnTrue()
    {
        // Arrange
        Mood firstMood = _moodFixture.Create(name: "calm");
        Mood secondMood = _moodFixture.Create(name: "calm");

        // Act
        bool result = firstMood.Equals(secondMood);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        Mood firstMood = _moodFixture.Create(name: "calm");
        Mood secondMood = _moodFixture.Create(name: "energetic");

        // Act
        bool result = firstMood.Equals(secondMood);

        // Assert
        Assert.False(result);
    }
}
