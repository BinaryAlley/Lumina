#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Validators;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="ThemeEngineOptionsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeEngineOptionsDtoValidatorTests
{
    private readonly ThemeEngineOptionsDtoValidator _validator = new();
    private readonly ThemeEngineOptionsDtoFixture _themeEngineOptionsDtoFixture = new();

    [Fact]
    public void Validate_WhenAllPropertiesValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create();

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenStoragePathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(storagePath: string.Empty);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeStoragePathCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenBundledThemesPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(bundledThemesPath: string.Empty);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeBundledThemesPathCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void Validate_WhenDefaultThemeIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(defaultThemeId: string.Empty);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeDefaultThemeIdCannotBeEmpty.Description, result[0].Description);
    }

    [Theory]
    [InlineData(0L)] // zero, not positive
    [InlineData(-1L)] // negative
    public void Validate_WhenMaxArchiveBytesIsNotPositive_ShouldHaveValidationError(long maxArchiveBytes)
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(maxArchiveBytes: maxArchiveBytes);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeMaxArchiveBytesMustBePositive.Description, result[0].Description);
    }

    [Theory]
    [InlineData(0L)] // zero, not positive
    [InlineData(-1L)] // negative
    public void Validate_WhenMaxExpandedBytesIsNotPositive_ShouldHaveValidationError(long maxExpandedBytes)
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(maxExpandedBytes: maxExpandedBytes);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeMaxExpandedBytesMustBePositive.Description, result[0].Description);
    }

    [Theory]
    [InlineData(0L)] // zero, not positive
    [InlineData(-1L)] // negative
    public void Validate_WhenMaxSingleFileBytesIsNotPositive_ShouldHaveValidationError(long maxSingleFileBytes)
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(maxSingleFileBytes: maxSingleFileBytes);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeMaxSingleFileBytesMustBePositive.Description, result[0].Description);
    }

    [Theory]
    [InlineData(0)] // zero, not positive
    [InlineData(-1)] // negative
    public void Validate_WhenMaxEntriesIsNotPositive_ShouldHaveValidationError(int maxEntries)
    {
        // Arrange
        ThemeEngineOptionsDto options = _themeEngineOptionsDtoFixture.Create(maxEntries: maxEntries);

        // Act
        List<Error> result = _validator.Validate(options);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.ThemeMaxEntriesMustBePositive.Description, result[0].Description);
    }
}
