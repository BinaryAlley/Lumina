#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSchedulerDisplayPreferencesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesRequestMappingTests
{
    private readonly UpdateSchedulerDisplayPreferencesRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesRequest request = _requestFixture.Create(
            jobTypeFilter: ScheduledTaskType.CleanTemporaryFiles,
            displayTimeSpan: 15,
            displayTimeUnit: SchedulerDisplayTimeUnit.Hours);

        // Act
        UpdateSchedulerDisplayPreferencesCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.JobTypeFilter, result.JobTypeFilter);
        Assert.Equal(request.DisplayTimeSpan, result.DisplayTimeSpan);
        Assert.Equal(request.DisplayTimeUnit, result.DisplayTimeUnit);
    }
}
