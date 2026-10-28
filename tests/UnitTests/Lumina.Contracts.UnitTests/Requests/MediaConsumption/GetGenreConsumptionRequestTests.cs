#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetGenreConsumptionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionRequestTests
{
    private readonly GetGenreConsumptionRequestFixture _getGenreConsumptionRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionRequestTests"/> class.
    /// </summary>
    public GetGenreConsumptionRequestTests()
    {
        _getGenreConsumptionRequestFixture = new GetGenreConsumptionRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.LibraryId);
        Assert.True(Guid.TryParse(sut.LibraryId, out Guid parsedLibraryId));
        Assert.NotEqual(Guid.Empty, parsedLibraryId);
        Assert.NotNull(sut.MediaKind);
        Assert.True(Enum.TryParse(sut.MediaKind, ignoreCase: true, out MediaKind parsedMediaKind));
        Assert.NotEqual(MediaKind.None, parsedMediaKind);
        Assert.True(Enum.IsDefined(parsedMediaKind));
        Assert.NotNull(sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
        Assert.True(DateTime.TryParse(sut.FromUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsedFromUtc));
        Assert.True(DateTime.TryParse(sut.ToUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsedToUtc));
        Assert.True(parsedFromUtc <= parsedToUtc);
    }

    [Fact]
    public void Constructor_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        string mediaKind = nameof(MediaKind.Book);
        string fromUtc = "2025-01-02T03:04:05.0000000Z";
        string toUtc = "2025-02-03T04:05:06.0000000Z";

        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: mediaKind,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Assert
        Assert.Equal(libraryId, sut.LibraryId);
        Assert.Equal(mediaKind, sut.MediaKind);
        Assert.Equal(fromUtc, sut.FromUtc);
        Assert.Equal(toUtc, sut.ToUtc);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(includeLibraryId: false);

        // Assert
        Assert.Null(sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(includeMediaKind: false);

        // Assert
        Assert.Null(sut.MediaKind);
    }

    [Fact]
    public void Constructor_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(includeFromUtc: false);

        // Assert
        Assert.Null(sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
    }

    [Fact]
    public void Constructor_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(includeToUtc: false);

        // Assert
        Assert.Null(sut.ToUtc);
        Assert.NotNull(sut.FromUtc);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsEmpty_ShouldPreserveEmpty()
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(libraryId: string.Empty);

        // Assert
        Assert.Equal(string.Empty, sut.LibraryId);
    }

    [Theory]
    [InlineData("999")] // a numeric value that is not a member name
    [InlineData("not-a-media-kind")] // a textual value that is not a member name
    [InlineData("")] // an empty media kind
    public void Constructor_WhenMediaKindIsInvalid_ShouldPreserveItForTheValidator(string mediaKind)
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(mediaKind: mediaKind);

        // Assert
        Assert.Equal(mediaKind, sut.MediaKind);
    }

    [Fact]
    public void Constructor_WhenDateBoundaryValuesAreSupplied_ShouldPreserveThem()
    {
        // Arrange
        string fromUtc = DateTime.MinValue.ToString("o", CultureInfo.InvariantCulture);
        string toUtc = DateTime.MaxValue.ToString("o", CultureInfo.InvariantCulture);

        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Assert
        Assert.Equal(fromUtc, sut.FromUtc);
        Assert.Equal(toUtc, sut.ToUtc);
    }

    [Theory]
    [InlineData("not-a-date")] // a textual value that is not a date
    [InlineData("2026-13-45T99:99:99")] // a value with out of range components
    public void Constructor_WhenDateBoundsAreMalformed_ShouldPreserveThemForTheValidator(string malformedBound)
    {
        // Act
        GetGenreConsumptionRequest sut = _getGenreConsumptionRequestFixture.Create(
            fromUtc: malformedBound,
            toUtc: malformedBound);

        // Assert
        Assert.Equal(malformedBound, sut.FromUtc);
        Assert.Equal(malformedBound, sut.ToUtc);
    }
}
