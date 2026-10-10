#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="ReleaseInfoDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReleaseInfoDtoMappingTests
{
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingCompleteReleaseInfoDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        ReleaseInfoDto dto = _releaseInfoDtoFixture.Create(
            originalReleaseDate: new DateOnly(2000, 1, 1),
            originalReleaseYear: 2000,
            reReleaseDate: new DateOnly(2010, 1, 1),
            reReleaseYear: 2010,
            releaseCountry: ReleaseCountry.AD,
            releaseVersion: "Director's Cut");

        // Act
        Result<ReleaseInfo> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.OriginalReleaseDate.HasValue);
        Assert.Equal(dto.OriginalReleaseDate!.Value, result.Value.OriginalReleaseDate.Value);
        Assert.True(result.Value.OriginalReleaseYear.HasValue);
        Assert.Equal(dto.OriginalReleaseYear!.Value, result.Value.OriginalReleaseYear.Value);
        Assert.True(result.Value.ReReleaseDate.HasValue);
        Assert.Equal(dto.ReReleaseDate!.Value, result.Value.ReReleaseDate.Value);
        Assert.True(result.Value.ReReleaseYear.HasValue);
        Assert.Equal(dto.ReReleaseYear!.Value, result.Value.ReReleaseYear.Value);
        Assert.True(result.Value.ReleaseCountry.HasValue);
        Assert.Equal(dto.ReleaseCountry!.Value, result.Value.ReleaseCountry.Value);
        Assert.True(result.Value.ReleaseVersion.HasValue);
        Assert.Equal(dto.ReleaseVersion, result.Value.ReleaseVersion.Value);
    }

    [Fact]
    public void ToDomainValueObject_WhenOptionalValuesAreMissing_ShouldMapWithoutOptionalValues()
    {
        // Arrange
        ReleaseInfoDto dto = _releaseInfoDtoFixture.Create();

        // Act
        Result<ReleaseInfo> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.OriginalReleaseDate.HasValue);
        Assert.False(result.Value.OriginalReleaseYear.HasValue);
        Assert.False(result.Value.ReReleaseDate.HasValue);
        Assert.False(result.Value.ReReleaseYear.HasValue);
        Assert.False(result.Value.ReleaseCountry.HasValue);
        Assert.False(result.Value.ReleaseVersion.HasValue);
    }

    [Fact]
    public void ToDomainValueObject_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldReturnError()
    {
        // Arrange
        ReleaseInfoDto dto = _releaseInfoDtoFixture.Create(
            originalReleaseDate: new DateOnly(2025, 1, 1),
            originalReleaseYear: 2024);

        // Act
        Result<ReleaseInfo> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainValueObject_WhenReReleaseDateAndYearDoNotMatch_ShouldReturnError()
    {
        // Arrange
        ReleaseInfoDto dto = _releaseInfoDtoFixture.Create(
            reReleaseDate: new DateOnly(2025, 1, 1),
            reReleaseYear: 2024);

        // Act
        Result<ReleaseInfo> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
    }
}
