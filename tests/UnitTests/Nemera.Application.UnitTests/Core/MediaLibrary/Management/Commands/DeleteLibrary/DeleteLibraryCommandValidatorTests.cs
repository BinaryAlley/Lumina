#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using Nemera.Application.Fixtures.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.Management.Commands.DeleteLibrary;

/// <summary>
/// Contains unit tests for the <see cref="DeleteLibraryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryCommandValidatorTests
{
    private readonly DeleteLibraryCommandValidator _validator = new();
    private readonly DeleteLibraryCommandFixture _deleteLibraryCommandFixture = new();

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        DeleteLibraryCommand command = _deleteLibraryCommandFixture.Create();
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
        DeleteLibraryCommand command = _deleteLibraryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
