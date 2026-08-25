#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using Lumina.Application.Fixtures.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.ReorderLibraryArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryArtworkProvidersCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersCommandValidatorTests
{
    private readonly ReorderLibraryArtworkProvidersCommandValidator _validator = new();
    private readonly ReorderLibraryArtworkProvidersCommandFixture _reorderLibraryArtworkProvidersCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ReorderLibraryArtworkProvidersCommand command = _reorderLibraryArtworkProvidersCommandFixture.Create();
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
        ReorderLibraryArtworkProvidersCommand command = _reorderLibraryArtworkProvidersCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ReorderLibraryArtworkProvidersCommand command = _reorderLibraryArtworkProvidersCommandFixture.Create();
        command = command with { PluginIds = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginIdsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenPluginIdsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ReorderLibraryArtworkProvidersCommand command = _reorderLibraryArtworkProvidersCommandFixture.Create();
        command = command with { PluginIds = [] };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.PluginIdsListCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ReorderLibraryArtworkProvidersCommand command = _reorderLibraryArtworkProvidersCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.PluginIdsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Plugins.PluginIdsListCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ReorderLibraryArtworkProvidersCommand command = _reorderLibraryArtworkProvidersCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
