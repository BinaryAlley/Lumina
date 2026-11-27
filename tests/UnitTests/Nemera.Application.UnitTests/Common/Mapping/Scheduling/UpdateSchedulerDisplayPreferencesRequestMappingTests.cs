#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
using Nemera.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSchedulerDisplayPreferencesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesRequestMappingTests
{
    private readonly UpdateSchedulerDisplayPreferencesRequestFixture _updateSchedulerDisplayPreferencesRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create(
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
