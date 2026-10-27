#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ConsumedItemDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumedItemDtoMappingTests
{
    private readonly ConsumedItemDtoFixture _consumedItemDtoFixture = new();
    private readonly PaginatedResultDtoFixture<ConsumedItemDto> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingConsumedItem_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2024, 5, 17, 8, 30, 0, DateTimeKind.Utc);
        ConsumedItemDto item = _consumedItemDtoFixture.Create(
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId,
            libraryId: libraryId,
            isAvailable: false,
            title: "The Left Hand of Darkness",
            collectionTitle: "Hainish Cycle",
            creatorName: "Ursula K. Le Guin",
            coverPath: "covers/book-1.jpg",
            lastConsumedOnUtc: lastConsumedOnUtc,
            countedSessionCount: 4,
            completedSessionCount: 3,
            consumedDurationMilliseconds: 1_500);

        // Act
        ConsumedItemResponse result = item.ToResponse();

        // Assert
        Assert.Equal(MediaKind.Book, result.MediaKind);
        Assert.Equal(mediaItemId, result.MediaItemId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.False(result.IsAvailable);
        Assert.Equal("The Left Hand of Darkness", result.Title);
        Assert.Equal("Hainish Cycle", result.CollectionTitle);
        Assert.Equal("Ursula K. Le Guin", result.CreatorName);
        Assert.Equal("covers/book-1.jpg", result.CoverPath);
        Assert.Equal(lastConsumedOnUtc, result.LastConsumedOnUtc);
        Assert.Equal(4, result.CountedSessionCount);
        Assert.Equal(3, result.CompletedSessionCount);
        Assert.Equal(1.5, result.ConsumedDurationInSeconds);
    }

    [Fact]
    public void ToResponse_WhenLastConsumedOnUtcIsUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime lastConsumedOnUtc = new(2024, 2, 29, 23, 59, 58, DateTimeKind.Unspecified);
        ConsumedItemDto item = _consumedItemDtoFixture.Create(lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ConsumedItemResponse result = item.ToResponse();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.LastConsumedOnUtc.Kind);
        Assert.Equal(lastConsumedOnUtc.Ticks, result.LastConsumedOnUtc.Ticks);
    }

    [Fact]
    public void ToResponse_WhenDurationIsWholeMilliseconds_ShouldConvertToSeconds()
    {
        // Arrange
        ConsumedItemDto item = _consumedItemDtoFixture.Create(consumedDurationMilliseconds: 1_500);

        // Act
        ConsumedItemResponse result = item.ToResponse();

        // Assert
        Assert.Equal(1.5, result.ConsumedDurationInSeconds);
    }

    [Fact]
    public void ToResponse_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        ConsumedItemDto item = _consumedItemDtoFixture.Create(consumedDurationMilliseconds: long.MaxValue);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        ConsumedItemResponse result = item.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.ConsumedDurationInSeconds);
        Assert.True(result.ConsumedDurationInSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }

    [Fact]
    public void ToResponses_WhenMappingPaginatedItems_ShouldMapEveryItemAndPreserveMetadata()
    {
        // Arrange
        List<ConsumedItemDto> items = _consumedItemDtoFixture.CreateMany(3);
        PaginatedResultDto<ConsumedItemDto> paginatedItems = _paginatedResultDtoFixture.Create(
            data: items,
            currentPage: 2,
            perPage: 3,
            count: 7,
            numberOfPages: 3);

        // Act
        PaginatedResponse<ConsumedItemResponse> result = paginatedItems.ToResponses();

        // Assert
        Assert.Equal(items.Count, result.Data.Count);
        for (int index = 0; index < items.Count; index++)
        {
            Assert.Equal(items[index].MediaItemId, result.Data[index].MediaItemId);
            Assert.Equal(items[index].Title, result.Data[index].Title);
            Assert.Equal(items[index].MediaKind, result.Data[index].MediaKind);
        }
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.PerPage);
        Assert.Equal(7, result.Count);
        Assert.Equal(3, result.NumberOfPages);
    }

    [Fact]
    public void ToResponses_WhenPaginatedItemsAreEmpty_ShouldReturnEmptyDataAndPreserveMetadata()
    {
        // Arrange
        PaginatedResultDto<ConsumedItemDto> paginatedItems = _paginatedResultDtoFixture.Create(
            data: [],
            currentPage: 1,
            perPage: 10,
            count: 0,
            numberOfPages: 0);

        // Act
        PaginatedResponse<ConsumedItemResponse> result = paginatedItems.ToResponses();

        // Assert
        Assert.Empty(result.Data);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PerPage);
        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.NumberOfPages);
    }
}
