#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryMetadataProviderEnabledCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledCommandValidatorTests
{
    private readonly SetLibraryMetadataProviderEnabledCommandValidator _validator = new();
    private readonly SetLibraryMetadataProviderEnabledCommandFixture _setLibraryMetadataProviderEnabledCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledCommand command = _setLibraryMetadataProviderEnabledCommandFixture.Create();
        command = command with { LibraryId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledCommand command = _setLibraryMetadataProviderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledCommand command = _setLibraryMetadataProviderEnabledCommandFixture.Create();
        command = command with { PluginId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledCommand command = _setLibraryMetadataProviderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledCommand command = _setLibraryMetadataProviderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
