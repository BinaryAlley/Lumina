#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.StartScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StartScheduledJobCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobCommandValidatorTests
{
    private readonly StartScheduledJobCommandValidator _validator = new();
    private readonly StartScheduledJobCommandFixture _startScheduledJobCommandFixture = new();

    [Fact]
    public void Validate_WhenScheduledJobIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create() with { ScheduledJobId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.ScheduledJobNotFound);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
