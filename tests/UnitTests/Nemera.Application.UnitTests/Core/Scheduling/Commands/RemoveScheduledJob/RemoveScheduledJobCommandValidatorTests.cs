#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.RemoveScheduledJob;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.RemoveScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="RemoveScheduledJobCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobCommandValidatorTests
{
    private readonly RemoveScheduledJobCommandValidator _validator = new();
    private readonly RemoveScheduledJobCommandFixture _removeScheduledJobCommandFixture = new();

    [Fact]
    public void Validate_WhenScheduledJobIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create() with { ScheduledJobId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.ScheduledJobNotFound);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
