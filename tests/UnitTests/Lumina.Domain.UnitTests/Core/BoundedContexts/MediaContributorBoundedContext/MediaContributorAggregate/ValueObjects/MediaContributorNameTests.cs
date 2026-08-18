#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorName"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorNameTests
{
    private readonly MediaContributorNameFixture _mediaContributorNameFixture = new();

    [Fact]
    public void Create_WhenCalledWithDisplayNameAndLegalName_ShouldCreateName()
    {
        // Act
        Result<MediaContributorName> result = MediaContributorName.Create("Jane Doe", Optional<string>.Some("Jane Doe Smith"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Jane Doe", result.Value.DisplayName);
        Assert.True(result.Value.LegalName.HasValue);
        Assert.Equal("Jane Doe Smith", result.Value.LegalName.Value);
    }

    [Fact]
    public void Create_WhenCalledWithoutLegalName_ShouldCreateNameWithoutLegalName()
    {
        // Act
        Result<MediaContributorName> result = MediaContributorName.Create("Jane Doe", Optional<string>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Jane Doe", result.Value.DisplayName);
        Assert.False(result.Value.LegalName.HasValue);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MediaContributorName firstName = _mediaContributorNameFixture.Create(displayName: "Jane Doe", legalName: Optional<string>.None());
        MediaContributorName secondName = _mediaContributorNameFixture.Create(displayName: "Jane Doe", legalName: Optional<string>.None());

        // Act
        bool result = firstName.Equals(secondName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentDisplayName_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorName firstName = _mediaContributorNameFixture.Create(displayName: "Jane Doe", legalName: Optional<string>.None());
        MediaContributorName secondName = _mediaContributorNameFixture.Create(displayName: "John Smith", legalName: Optional<string>.None());

        // Act
        bool result = firstName.Equals(secondName);

        // Assert
        Assert.False(result);
    }
}
