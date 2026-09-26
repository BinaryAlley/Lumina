#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsrcDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcDtoMappingTests
{
    private readonly IsrcDtoFixture _isrcDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidIsrcDto_ShouldMapCorrectly()
    {
        // Arrange
        IsrcDto dto = _isrcDtoFixture.Create();

        // Act
        Result<Isrc> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value!.Trim().ToUpperInvariant(), result.Value.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingLowercaseIsrcDto_ShouldNormalizeTheValue()
    {
        // Arrange
        IsrcDto dto = _isrcDtoFixture.Create(value: "gbum71029604");

        // Act
        Result<Isrc> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("GBUM71029604", result.Value.Value);
    }

    [Theory]
    [InlineData(null)] // missing value
    [InlineData("")] // empty value
    [InlineData("   ")] // whitespace value
    public void ToDomainEntity_WhenValueIsNullOrWhitespace_ShouldReturnError(string? value)
    {
        // Arrange
        IsrcDto dto = _isrcDtoFixture.Create(value: value, includeValue: value is not null);

        // Act
        Result<Isrc> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.IsrcValueCannotBeEmpty, result.FirstError);
    }

    [Theory]
    [InlineData("invalid")] // not an ISRC at all
    [InlineData("GBUM7102960")] // too short
    [InlineData("GBUM71029604X")] // too long
    [InlineData("1BUM71029604")] // country code is not two letters
    [InlineData("GBUM7102960A")] // designation code is not all digits
    public void ToDomainEntity_WhenValueIsMalformed_ShouldReturnError(string value)
    {
        // Arrange
        IsrcDto dto = _isrcDtoFixture.Create(value: value);

        // Act
        Result<Isrc> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.IsrcInvalidFormat, result.FirstError);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidIsrcDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<IsrcDto> dtos = _isrcDtoFixture.CreateMany(2);

        // Act
        List<Result<Isrc>> results = [.. dtos.ToDomainEntities()];

        // Assert
        Assert.Equal(dtos.Count, results.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            Assert.False(results[i].IsFailure);
            Assert.Equal(dtos[i].Value!.Trim().ToUpperInvariant(), results[i].Value.Value);
        }
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMixedValidAndInvalidIsrcDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<IsrcDto> dtos =
        [
            _isrcDtoFixture.Create(value: "GBUM71029604"),
            _isrcDtoFixture.Create(value: "invalid"),
            _isrcDtoFixture.Create(value: "USRC17607839")
        ];

        // Act
        List<Result<Isrc>> results = [.. dtos.ToDomainEntities()];

        // Assert
        Assert.Equal(dtos.Count, results.Count);
        Assert.False(results[0].IsFailure);
        Assert.True(results[1].IsFailure);
        Assert.False(results[2].IsFailure);
    }
}
