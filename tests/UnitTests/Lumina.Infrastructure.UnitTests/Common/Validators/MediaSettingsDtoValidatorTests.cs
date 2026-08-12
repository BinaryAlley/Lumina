#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Validators;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="MediaSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaSettingsDtoValidatorTests
{
    private readonly MediaSettingsDtoValidator _validator;
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSettingsDtoValidatorTests"/> class.
    /// </summary>
    public MediaSettingsDtoValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenRootDirectoryProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.RootDirectory, "/path/to/media")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenRootDirectoryIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.RootDirectory, string.Empty)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(Errors.Configuration.MediaRootDirectoryCannotBeEmpty.Description, result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenRootDirectoryIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.RootDirectory, "   ")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(Errors.Configuration.MediaRootDirectoryCannotBeEmpty.Description, result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenMediaLibrariesDirectoryIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.LibrariesDirectory, string.Empty)
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(Errors.Configuration.MediaLibrariesDirectoryCannotBeEmpty.Description, result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenMediaLibrariesDirectoryProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.LibrariesDirectory, "/path/to/media")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenMediaLibrariesDirectoryIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.LibrariesDirectory, "   ")
            .Create();

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(Errors.Configuration.MediaLibrariesDirectoryCannotBeEmpty.Description, result.Errors[0].ErrorMessage);
    }
}
