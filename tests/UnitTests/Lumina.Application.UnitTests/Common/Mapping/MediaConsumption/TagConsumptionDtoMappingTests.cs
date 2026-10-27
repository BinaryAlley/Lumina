#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="TagConsumptionDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagConsumptionDtoMappingTests
{
    private readonly TagConsumptionDtoFixture _tagConsumptionDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingTagConsumption_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        TagConsumptionDto tagConsumption = _tagConsumptionDtoFixture.Create(
            name: "science-fiction",
            consumedItemCount: 17,
            consumedDurationMilliseconds: 4_500);

        // Act
        TagConsumptionResponse result = tagConsumption.ToResponse();

        // Assert
        Assert.Equal("science-fiction", result.Name);
        Assert.Equal(17, result.ConsumedItemCount);
        Assert.Equal(4.5, result.ConsumedDurationInSeconds);
    }

    [Fact]
    public void ToResponse_WhenDurationExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        TagConsumptionDto tagConsumption = _tagConsumptionDtoFixture.Create(consumedDurationMilliseconds: long.MaxValue);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        TagConsumptionResponse result = tagConsumption.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.ConsumedDurationInSeconds);
        Assert.True(result.ConsumedDurationInSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleTagConsumptions_ShouldMapThemInOrder()
    {
        // Arrange
        List<TagConsumptionDto> tagConsumptions =
        [
            _tagConsumptionDtoFixture.Create(name: "first", consumedItemCount: 1, consumedDurationMilliseconds: 1_000),
            _tagConsumptionDtoFixture.Create(name: "second", consumedItemCount: 2, consumedDurationMilliseconds: 2_000),
            _tagConsumptionDtoFixture.Create(name: "third", consumedItemCount: 3, consumedDurationMilliseconds: 3_000)
        ];

        // Act
        IReadOnlyList<TagConsumptionResponse> result = tagConsumptions.ToResponses();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("first", result[0].Name);
        Assert.Equal(1, result[0].ConsumedItemCount);
        Assert.Equal(1.0, result[0].ConsumedDurationInSeconds);
        Assert.Equal("second", result[1].Name);
        Assert.Equal(2, result[1].ConsumedItemCount);
        Assert.Equal(2.0, result[1].ConsumedDurationInSeconds);
        Assert.Equal("third", result[2].Name);
        Assert.Equal(3, result[2].ConsumedItemCount);
        Assert.Equal(3.0, result[2].ConsumedDurationInSeconds);
    }

    [Fact]
    public void ToResponses_WhenInputIsEmpty_ShouldReturnAnEmptyList()
    {
        // Arrange
        List<TagConsumptionDto> tagConsumptions = [];

        // Act
        IReadOnlyList<TagConsumptionResponse> result = tagConsumptions.ToResponses();

        // Assert
        Assert.Empty(result);
    }
}
