#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryBookReaderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryBookReaderEnabledCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledCommandValidatorTests
{
    private readonly SetLibraryBookReaderEnabledCommandValidator _validator = new();
    private readonly SetLibraryBookReaderEnabledCommandFixture _setLibraryBookReaderEnabledCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create(libraryId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create(pluginId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Plugins.PluginIdCannotBeEmpty);
    }
}
