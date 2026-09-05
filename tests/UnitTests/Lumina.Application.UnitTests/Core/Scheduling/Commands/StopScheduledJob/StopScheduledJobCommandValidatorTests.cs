#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.StopScheduledJob;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.StopScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StopScheduledJobCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobCommandValidatorTests
{
    private readonly StopScheduledJobCommandValidator _validator = new();
    private readonly StopScheduledJobCommandFixture _stopScheduledJobCommandFixture = new();

    [Fact]
    public void Validate_WhenScheduledJobIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create() with { ScheduledJobId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.ScheduledJobNotFound);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
