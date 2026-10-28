#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionItemHistoryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryRequestTests
{
    private readonly DeleteConsumptionItemHistoryRequestFixture _deleteConsumptionItemHistoryRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryRequestTests"/> class.
    /// </summary>
    public DeleteConsumptionItemHistoryRequestTests()
    {
        _deleteConsumptionItemHistoryRequestFixture = new DeleteConsumptionItemHistoryRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        DeleteConsumptionItemHistoryRequest sut = _deleteConsumptionItemHistoryRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.True(Enum.TryParse(sut.MediaKind, ignoreCase: true, out MediaKind parsedMediaKind));
        Assert.NotEqual(MediaKind.None, parsedMediaKind);
        Assert.True(Enum.IsDefined(parsedMediaKind));
        Assert.NotNull(sut.MediaItemId);
        Assert.True(Guid.TryParse(sut.MediaItemId, out Guid parsedMediaItemId));
        Assert.NotEqual(Guid.Empty, parsedMediaItemId);
        Assert.NotNull(sut.LibraryId);
        Assert.True(Guid.TryParse(sut.LibraryId, out Guid parsedLibraryId));
        Assert.NotEqual(Guid.Empty, parsedLibraryId);
    }

    [Fact]
    public void Constructor_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        string mediaKind = nameof(MediaKind.Book);
        string mediaItemId = Guid.NewGuid().ToString();
        string libraryId = Guid.NewGuid().ToString();

        // Act
        DeleteConsumptionItemHistoryRequest sut = _deleteConsumptionItemHistoryRequestFixture.Create(
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            libraryId: libraryId);

        // Assert
        Assert.Equal(mediaKind, sut.MediaKind);
        Assert.Equal(mediaItemId, sut.MediaItemId);
        Assert.Equal(libraryId, sut.LibraryId);
    }

    [Theory]
    [InlineData("None")] // None is not a valid media kind for a consumption record
    [InlineData("999")] // a numeric value that is not a member name
    [InlineData("not-a-media-kind")] // a textual value that is not a member name
    [InlineData("")] // an empty media kind
    public void Constructor_WhenMediaKindIsInvalid_ShouldPreserveItForTheValidator(string mediaKind)
    {
        // Act
        DeleteConsumptionItemHistoryRequest sut = _deleteConsumptionItemHistoryRequestFixture.Create(mediaKind: mediaKind);

        // Assert
        Assert.Equal(mediaKind, sut.MediaKind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Constructor_WhenMediaItemIdIsInvalid_ShouldPreserveItForTheValidator(string mediaItemId)
    {
        // Act
        DeleteConsumptionItemHistoryRequest sut = _deleteConsumptionItemHistoryRequestFixture.Create(mediaItemId: mediaItemId);

        // Assert
        Assert.Equal(mediaItemId, sut.MediaItemId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Constructor_WhenLibraryIdIsInvalid_ShouldPreserveItForTheValidator(string libraryId)
    {
        // Act
        DeleteConsumptionItemHistoryRequest sut = _deleteConsumptionItemHistoryRequestFixture.Create(libraryId: libraryId);

        // Assert
        Assert.Equal(libraryId, sut.LibraryId);
    }
}
