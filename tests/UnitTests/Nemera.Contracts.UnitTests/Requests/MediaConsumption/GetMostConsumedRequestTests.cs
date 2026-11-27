#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetMostConsumedRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedRequestTests
{
    private readonly GetMostConsumedRequestFixture _getMostConsumedRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedRequestTests"/> class.
    /// </summary>
    public GetMostConsumedRequestTests()
    {
        _getMostConsumedRequestFixture = new GetMostConsumedRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.LibraryId);
        Assert.True(Guid.TryParse(sut.LibraryId, out Guid parsedLibraryId));
        Assert.NotEqual(Guid.Empty, parsedLibraryId);
        Assert.NotNull(sut.MediaKind);
        Assert.NotEqual(nameof(MediaKind.None), sut.MediaKind);
        Assert.True(sut.CurrentPage.HasValue);
        Assert.True(sut.CurrentPage.Value > 0);
        Assert.True(sut.PerPage.HasValue);
        Assert.True(sut.PerPage.Value > 0);
        Assert.NotNull(sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
        DateTime parsedFromUtc = DateTime.Parse(sut.FromUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        DateTime parsedToUtc = DateTime.Parse(sut.ToUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        Assert.True(parsedFromUtc <= parsedToUtc);
    }

    [Fact]
    public void Constructor_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2025-01-02T03:04:05Z";
        string toUtc = "2025-02-03T04:05:06Z";

        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Movie),
            currentPage: 3,
            perPage: 25,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Assert
        Assert.Equal(libraryId, sut.LibraryId);
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(nameof(MediaKind.Movie), sut.MediaKind);
        Assert.True(sut.CurrentPage.HasValue);
        Assert.Equal(3, sut.CurrentPage.Value);
        Assert.True(sut.PerPage.HasValue);
        Assert.Equal(25, sut.PerPage.Value);
        Assert.NotNull(sut.FromUtc);
        Assert.Equal(fromUtc, sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
        Assert.Equal(toUtc, sut.ToUtc);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(includeLibraryId: false);

        // Assert
        Assert.Null(sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(includeMediaKind: false);

        // Assert
        Assert.Null(sut.MediaKind);
    }

    [Fact]
    public void Constructor_WhenCurrentPageIsNull_ShouldPreserveNull()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(includeCurrentPage: false);

        // Assert
        Assert.Null(sut.CurrentPage);
        Assert.True(sut.PerPage.HasValue);
    }

    [Fact]
    public void Constructor_WhenPerPageIsNull_ShouldPreserveNull()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(includePerPage: false);

        // Assert
        Assert.Null(sut.PerPage);
        Assert.True(sut.CurrentPage.HasValue);
    }

    [Fact]
    public void Constructor_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(includeFromUtc: false);

        // Assert
        Assert.Null(sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
    }

    [Fact]
    public void Constructor_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(includeToUtc: false);

        // Assert
        Assert.Null(sut.ToUtc);
        Assert.NotNull(sut.FromUtc);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsEmpty_ShouldPreserveEmpty()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(libraryId: string.Empty);

        // Assert
        Assert.Equal(string.Empty, sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenPaginationBoundaryValuesAreSupplied_ShouldPreserveThem()
    {
        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(
            currentPage: 0,
            perPage: -5);

        // Assert
        Assert.True(sut.CurrentPage.HasValue);
        Assert.Equal(0, sut.CurrentPage.Value);
        Assert.True(sut.PerPage.HasValue);
        Assert.Equal(-5, sut.PerPage.Value);
    }

    [Fact]
    public void Constructor_WhenDateBoundaryValuesAreSupplied_ShouldPreserveThem()
    {
        // Arrange
        string fromUtc = "0001-01-01T00:00:00Z";
        string toUtc = "9999-12-31T23:59:59.9999999Z";

        // Act
        GetMostConsumedRequest sut = _getMostConsumedRequestFixture.Create(
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Assert
        Assert.NotNull(sut.FromUtc);
        Assert.Equal(fromUtc, sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
        Assert.Equal(toUtc, sut.ToUtc);
    }
}
