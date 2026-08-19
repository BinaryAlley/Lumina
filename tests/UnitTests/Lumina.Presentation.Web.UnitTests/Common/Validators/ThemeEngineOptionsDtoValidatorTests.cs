#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Validators;
using Lumina.Presentation.Web.UnitTests.Common.Setup;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="ThemeEngineOptionsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeEngineOptionsDtoValidatorTests
{
    private readonly ThemeEngineOptionsDtoValidator _validator = new();

    [Fact]
    public void Validate_WhenStoragePathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { StoragePath = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Theme storage path cannot be empty!"));
    }

    [Fact]
    public void Validate_WhenSettingsPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { SettingsPath = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Theme settings path cannot be empty!"));
    }

    [Fact]
    public void Validate_WhenDefaultThemeIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { DefaultThemeId = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Default theme Id cannot be empty!"));
    }

    [Theory]
    [InlineData(0L)] // zero, not positive
    [InlineData(-1L)] // negative
    public void Validate_WhenMaxArchiveBytesIsNotPositive_ShouldHaveValidationError(long maxArchiveBytes)
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { MaxArchiveBytes = maxArchiveBytes };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Maximum archive size must be greater than 0!"));
    }

    [Theory]
    [InlineData(0L)] // zero, not positive
    [InlineData(-1L)] // negative
    public void Validate_WhenMaxExpandedBytesIsNotPositive_ShouldHaveValidationError(long maxExpandedBytes)
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { MaxExpandedBytes = maxExpandedBytes };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Maximum expanded size must be greater than 0!"));
    }

    [Theory]
    [InlineData(0L)] // zero, not positive
    [InlineData(-1L)] // negative
    public void Validate_WhenMaxSingleFileBytesIsNotPositive_ShouldHaveValidationError(long maxSingleFileBytes)
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { MaxSingleFileBytes = maxSingleFileBytes };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Maximum single file size must be greater than 0!"));
    }

    [Theory]
    [InlineData(0)] // zero, not positive
    [InlineData(-1)] // negative
    public void Validate_WhenMaxEntriesIsNotPositive_ShouldHaveValidationError(int maxEntries)
    {
        // Arrange
        ThemeEngineOptionsDto options = new() { MaxEntries = maxEntries };

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationError(Error.Validation(description: "Maximum entries count must be greater than 0!"));
    }

    [Fact]
    public void Validate_WhenOptionsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        ThemeEngineOptionsDto options = new();

        // Act
        List<Error> result = _validator.TestValidate(options);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
