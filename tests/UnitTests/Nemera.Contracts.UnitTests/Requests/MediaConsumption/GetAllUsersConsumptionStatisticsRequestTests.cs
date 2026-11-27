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
/// Contains unit tests for the <see cref="GetAllUsersConsumptionStatisticsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsRequestTests
{
    private readonly GetAllUsersConsumptionStatisticsRequestFixture _getAllUsersConsumptionStatisticsRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsRequestTests"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsRequestTests()
    {
        _getAllUsersConsumptionStatisticsRequestFixture = new GetAllUsersConsumptionStatisticsRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.LibraryId);
        Assert.True(Guid.TryParse(sut.LibraryId, out Guid parsedLibraryId));
        Assert.NotEqual(Guid.Empty, parsedLibraryId);
        Assert.NotNull(sut.MediaKind);
        Assert.NotEqual(nameof(MediaKind.None), sut.MediaKind);
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
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Audiobook),
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Assert
        Assert.Equal(libraryId, sut.LibraryId);
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(nameof(MediaKind.Audiobook), sut.MediaKind);
        Assert.NotNull(sut.FromUtc);
        Assert.Equal(fromUtc, sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
        Assert.Equal(toUtc, sut.ToUtc);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeLibraryId: false);

        // Assert
        Assert.Null(sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeMediaKind: false);

        // Assert
        Assert.Null(sut.MediaKind);
    }

    [Fact]
    public void Constructor_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeFromUtc: false);

        // Assert
        Assert.Null(sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
    }

    [Fact]
    public void Constructor_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeToUtc: false);

        // Assert
        Assert.Null(sut.ToUtc);
        Assert.NotNull(sut.FromUtc);
    }

    [Fact]
    public void Constructor_WhenLibraryIdIsEmpty_ShouldPreserveEmpty()
    {
        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(libraryId: string.Empty);

        // Assert
        Assert.Equal(string.Empty, sut.LibraryId);
    }

    [Fact]
    public void Constructor_WhenDateBoundaryValuesAreSupplied_ShouldPreserveThem()
    {
        // Arrange
        string fromUtc = "0001-01-01T00:00:00Z";
        string toUtc = "9999-12-31T23:59:59.9999999Z";

        // Act
        GetAllUsersConsumptionStatisticsRequest sut = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Assert
        Assert.NotNull(sut.FromUtc);
        Assert.Equal(fromUtc, sut.FromUtc);
        Assert.NotNull(sut.ToUtc);
        Assert.Equal(toUtc, sut.ToUtc);
    }
}
