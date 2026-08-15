#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Contains unit tests for the <see cref="CancelLibraryScanCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanCommandValidatorTests
{
    private readonly CancelLibraryScanCommandValidator _validator = new();
    private readonly CancelLibraryScanCommandFixture _cancelLibraryScanCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        command = command with { LibraryId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenScanIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        command = command with { ScanId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.LibraryScanning.ScanIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenScanIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.LibraryScanning.ScanIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
