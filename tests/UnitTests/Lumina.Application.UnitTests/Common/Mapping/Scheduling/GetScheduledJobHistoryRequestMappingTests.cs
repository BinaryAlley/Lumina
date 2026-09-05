#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobHistoryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryRequestMappingTests
{
    private readonly GetScheduledJobHistoryRequestFixture _requestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        DateTime from = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime to = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        GetScheduledJobHistoryRequest request = _requestFixture.Create(from, to);

        // Act
        GetScheduledJobHistoryQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.From, result.From);
        Assert.Equal(request.To, result.To);
    }
}
