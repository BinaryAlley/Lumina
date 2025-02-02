#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Models.Configuration;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Validators;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="MediaSettingsModelValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaSettingsModelValidatorTests
{
    private readonly MediaSettingsModelValidator _validator;
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSettingsModelValidatorTests"/> class.
    /// </summary>
    public MediaSettingsModelValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _validator = new();
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenRootDirectoryProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        MediaSettingsModel model = _fixture.Build<MediaSettingsModel>()
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
        MediaSettingsModel model = _fixture.Build<MediaSettingsModel>()
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
        MediaSettingsModel model = _fixture.Build<MediaSettingsModel>()
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
        MediaSettingsModel model = _fixture.Build<MediaSettingsModel>()
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
        MediaSettingsModel model = _fixture.Build<MediaSettingsModel>()
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
        MediaSettingsModel model = _fixture.Build<MediaSettingsModel>()
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
