#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobHistoryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryRequestMappingTests
{
    private readonly GetScheduledJobHistoryRequestFixture _getScheduledJobHistoryRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        DateTime from = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime to = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create(from, to);

        // Act
        GetScheduledJobHistoryQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.From, result.From);
        Assert.Equal(request.To, result.To);
    }
}
