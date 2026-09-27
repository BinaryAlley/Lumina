#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MusicMediaContributor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMediaContributorTests
{
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    [Fact]
    public void Create_WhenCalled_ShouldReturnContributorWithContributorIdAndRole()
    {
        // Arrange
        MediaContributorId contributorId = _mediaContributorIdFixture.Create();

        // Act
        MusicMediaContributor contributor = _musicMediaContributorFixture.Create(contributorId: contributorId, role: MediaContributorRole.Composer);

        // Assert
        Assert.Equal(contributorId, contributor.ContributorId);
        Assert.Equal(MediaContributorRole.Composer, contributor.Role);
    }

    [Fact]
    public void Equals_WithSameContributorIdAndRole_ShouldReturnTrue()
    {
        // Arrange
        MediaContributorId contributorId = _mediaContributorIdFixture.Create();
        MusicMediaContributor firstContributor = _musicMediaContributorFixture.Create(contributorId: contributorId, role: MediaContributorRole.Composer);
        MusicMediaContributor secondContributor = _musicMediaContributorFixture.Create(contributorId: contributorId, role: MediaContributorRole.Composer);

        // Act
        bool result = firstContributor.Equals(secondContributor);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentRole_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorId contributorId = _mediaContributorIdFixture.Create();
        MusicMediaContributor firstContributor = _musicMediaContributorFixture.Create(contributorId: contributorId, role: MediaContributorRole.Composer);
        MusicMediaContributor secondContributor = _musicMediaContributorFixture.Create(contributorId: contributorId, role: MediaContributorRole.Producer);

        // Act
        bool result = firstContributor.Equals(secondContributor);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentContributorId_ShouldReturnFalse()
    {
        // Arrange
        MusicMediaContributor firstContributor = _musicMediaContributorFixture.Create(contributorId: _mediaContributorIdFixture.Create(), role: MediaContributorRole.Composer);
        MusicMediaContributor secondContributor = _musicMediaContributorFixture.Create(contributorId: _mediaContributorIdFixture.Create(), role: MediaContributorRole.Composer);

        // Act
        bool result = firstContributor.Equals(secondContributor);

        // Assert
        Assert.False(result);
    }
}
