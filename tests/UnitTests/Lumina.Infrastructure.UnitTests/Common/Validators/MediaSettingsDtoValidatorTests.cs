#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Validators;
using System.Collections.Generic;
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
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenRootDirectoryIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.RootDirectory, string.Empty)
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.MediaRootDirectoryCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenRootDirectoryIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.RootDirectory, "   ")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.MediaRootDirectoryCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenMediaLibrariesDirectoryIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.LibrariesDirectory, string.Empty)
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.MediaLibrariesDirectoryCannotBeEmpty.Description, result[0].Description);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenMediaLibrariesDirectoryProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.LibrariesDirectory, "/path/to/media")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MediaSettingsModelValidator_WhenMediaLibrariesDirectoryIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        MediaSettingsDto model = _fixture.Build<MediaSettingsDto>()
            .With(x => x.LibrariesDirectory, "   ")
            .Create();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Single(result);
        Assert.Equal(Errors.Configuration.MediaLibrariesDirectoryCannotBeEmpty.Description, result[0].Description);
    }
}
