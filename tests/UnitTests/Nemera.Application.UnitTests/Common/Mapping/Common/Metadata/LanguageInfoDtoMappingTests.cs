#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Common.Metadata;
using Nemera.Contracts.DTO.Common;
using Nemera.Contracts.Fixtures.Core.DTO.Common;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Common.ValueObjects.Metadata;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="LanguageInfoDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LanguageInfoDtoMappingTests
{
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingCompleteLanguageInfoDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        LanguageInfoDto dto = _languageInfoDtoFixture.Create(languageCode: "EN", languageName: "English", nativeName: "English");

        // Act
        Result<LanguageInfo> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("en", result.Value.LanguageCode);
        Assert.Equal(dto.LanguageName, result.Value.LanguageName);
        Assert.True(result.Value.NativeName.HasValue);
        Assert.Equal(dto.NativeName, result.Value.NativeName.Value);
    }

    [Fact]
    public void ToDomainValueObject_WhenNativeNameIsMissing_ShouldMapWithoutNativeName()
    {
        // Arrange
        LanguageInfoDto dto = _languageInfoDtoFixture.Create(includeNativeName: false);

        // Act
        Result<LanguageInfo> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.LanguageCode!.ToLowerInvariant(), result.Value.LanguageCode);
        Assert.Equal(dto.LanguageName, result.Value.LanguageName);
        Assert.False(result.Value.NativeName.HasValue);
    }
}
