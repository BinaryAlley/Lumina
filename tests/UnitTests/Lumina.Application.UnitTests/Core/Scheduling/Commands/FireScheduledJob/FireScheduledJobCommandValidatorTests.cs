#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.FireScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="FireScheduledJobCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobCommandValidatorTests
{
    private readonly FireScheduledJobCommandValidator _validator = new();
    private readonly FireScheduledJobCommandFixture _fireScheduledJobCommandFixture = new();

    [Fact]
    public void Validate_WhenScheduledJobIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create() with { ScheduledJobId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Scheduling.ScheduledJobNotFound);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
