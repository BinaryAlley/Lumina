#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ClearResumePointsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsRequestTests
{
    private readonly ClearResumePointsRequestFixture _clearResumePointsRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsRequestTests"/> class.
    /// </summary>
    public ClearResumePointsRequestTests()
    {
        _clearResumePointsRequestFixture = new ClearResumePointsRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAGuidLibraryId()
    {
        // Act
        ClearResumePointsRequest sut = _clearResumePointsRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.LibraryId);
        Assert.True(Guid.TryParse(sut.LibraryId, out Guid parsedLibraryId));
        Assert.NotEqual(Guid.Empty, parsedLibraryId);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsSupplied_ShouldPreserveIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();

        // Act
        ClearResumePointsRequest sut = _clearResumePointsRequestFixture.Create(libraryId: libraryId.ToString());

        // Assert
        Assert.Equal(libraryId.ToString(), sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Act
        ClearResumePointsRequest sut = _clearResumePointsRequestFixture.Create(includeLibraryId: false);

        // Assert
        Assert.Null(sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsEmpty_ShouldPreserveEmpty()
    {
        // Act
        ClearResumePointsRequest sut = _clearResumePointsRequestFixture.Create(libraryId: string.Empty);

        // Assert
        Assert.Equal(string.Empty, sut.LibraryId);
    }
}
