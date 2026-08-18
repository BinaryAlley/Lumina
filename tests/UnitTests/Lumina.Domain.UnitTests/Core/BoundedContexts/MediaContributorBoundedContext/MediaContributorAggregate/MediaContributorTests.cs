#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorTests
{
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();
    private readonly MediaContributorNameFixture _mediaContributorNameFixture = new();
    private readonly MediaContributorRoleFixture _mediaContributorRoleFixture = new();

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndValues_ShouldCreateContributor()
    {
        // Arrange
        MediaContributorId id = _mediaContributorIdFixture.Create();
        MediaContributorName name = _mediaContributorNameFixture.Create(displayName: "Jane Doe", legalName: Optional<string>.Some("Jane Doe Smith"));
        MediaContributorRole role = _mediaContributorRoleFixture.Create(name: "Actor", category: "Crew");
        Optional<DateOnly> dateOfBirth = Optional<DateOnly>.Some(new DateOnly(1980, 1, 1));
        Optional<DateOnly> dateOfDeath = Optional<DateOnly>.None();

        // Act
        Result<MediaContributor> result = MediaContributor.Create(id, name, role, dateOfBirth, dateOfDeath);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(role, result.Value.Role);
        Assert.True(result.Value.DateOfBirth.HasValue);
        Assert.Equal(new DateOnly(1980, 1, 1), result.Value.DateOfBirth.Value);
        Assert.False(result.Value.DateOfDeath.HasValue);
        Assert.False(result.Value.Biography.HasValue);
    }

    [Fact]
    public void Create_WhenCalledWithOptionalValuesAbsent_ShouldCreateContributorWithoutThem()
    {
        // Arrange
        MediaContributorName name = _mediaContributorNameFixture.Create(displayName: "Jane Doe", legalName: Optional<string>.None());
        MediaContributorRole role = _mediaContributorRoleFixture.Create(name: "Actor", category: "Crew");

        // Act
        Result<MediaContributor> result = MediaContributor.Create(_mediaContributorIdFixture.Create(), name, role, Optional<DateOnly>.None(), Optional<DateOnly>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.DateOfBirth.HasValue);
        Assert.False(result.Value.DateOfDeath.HasValue);
    }

    [Fact]
    public void Create_WhenCalledWithoutId_ShouldThrowNotImplementedException()
    {
        // Arrange
        MediaContributorName name = _mediaContributorNameFixture.Create(displayName: "Jane Doe", legalName: Optional<string>.None());

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => MediaContributor.Create(name));
    }
}
