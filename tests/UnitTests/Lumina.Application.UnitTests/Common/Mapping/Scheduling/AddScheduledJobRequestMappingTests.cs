#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="AddScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobRequestMappingTests
{
    private readonly AddScheduledJobRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        AddScheduledJobRequest request = _requestFixture.Create(
            name: "Scan at 6am",
            taskType: ScheduledTaskType.ScanMediaLibraries,
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 6,
            minute: 0);

        // Act
        AddScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.TaskType, result.TaskType);
        Assert.Equal(request.ScheduleType, result.ScheduleType);
        Assert.Equal(request.IntervalMinutes, result.IntervalMinutes);
        Assert.Equal(request.Hour, result.Hour);
        Assert.Equal(request.Minute, result.Minute);
    }
}
