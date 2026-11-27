#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibraryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryCommandValidatorTests
{
    private readonly ScanLibraryCommandValidator _validator = new();
    private readonly ScanLibraryCommandFixture _scanLibraryCommandFixture = new();

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        command = command with { Id = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
