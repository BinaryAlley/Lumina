#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryArtworkProviderEnabledCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryArtworkProviderEnabledCommandValidatorTests
{
    private readonly SetLibraryArtworkProviderEnabledCommandValidator _validator = new();
    private readonly SetLibraryArtworkProviderEnabledCommandFixture _setLibraryArtworkProviderEnabledCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledCommand command = _setLibraryArtworkProviderEnabledCommandFixture.Create();
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
        SetLibraryArtworkProviderEnabledCommand command = _setLibraryArtworkProviderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledCommand command = _setLibraryArtworkProviderEnabledCommandFixture.Create();
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
        SetLibraryArtworkProviderEnabledCommand command = _setLibraryArtworkProviderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledCommand command = _setLibraryArtworkProviderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
