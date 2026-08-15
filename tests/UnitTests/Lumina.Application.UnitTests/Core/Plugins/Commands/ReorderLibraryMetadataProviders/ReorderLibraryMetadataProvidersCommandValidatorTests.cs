#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Application.Fixtures.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersCommandValidatorTests
{
    private readonly ReorderLibraryMetadataProvidersCommandValidator _validator = new();
    private readonly ReorderLibraryMetadataProvidersCommandFixture _reorderLibraryMetadataProvidersCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ReorderLibraryMetadataProvidersCommand command = _reorderLibraryMetadataProvidersCommandFixture.Create();
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
        ReorderLibraryMetadataProvidersCommand command = _reorderLibraryMetadataProvidersCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPluginIdsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ReorderLibraryMetadataProvidersCommand command = _reorderLibraryMetadataProvidersCommandFixture.Create();
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
        ReorderLibraryMetadataProvidersCommand command = _reorderLibraryMetadataProvidersCommandFixture.Create();
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
        ReorderLibraryMetadataProvidersCommand command = _reorderLibraryMetadataProvidersCommandFixture.Create();

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
        ReorderLibraryMetadataProvidersCommand command = _reorderLibraryMetadataProvidersCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
