#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetSchedulerDisplayPreferences;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Scheduler.GetSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetSchedulerDisplayPreferencesEndpoint _sut;
    private readonly SchedulerDisplayPreferencesDtoFixture _schedulerDisplayPreferencesDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    public GetSchedulerDisplayPreferencesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetSchedulerDisplayPreferencesEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPreferences_ShouldReturnSuccessJsonWithDisplayPreferences()
    {
        // Arrange
        SchedulerDisplayPreferencesDto expectedPreferences = _schedulerDisplayPreferencesDtoFixture.Create(displayTimeUnit: Lumina.Presentation.Web.Common.Enums.Scheduling.SchedulerDisplayTimeUnit.Minutes);
        _mockApiHttpClient.GetAsync<SchedulerDisplayPreferencesDto>(ApiRoutes.ScheduledJobs.GET_DISPLAY_PREFERENCES, Arg.Any<CancellationToken>())
            .Returns(expectedPreferences);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPreferences.UserId, jsonDocument.RootElement.GetProperty("data").GetProperty("userId").GetGuid());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestDisplayPreferencesFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<SchedulerDisplayPreferencesDto>(ApiRoutes.ScheduledJobs.GET_DISPLAY_PREFERENCES, Arg.Any<CancellationToken>())
            .Returns(_schedulerDisplayPreferencesDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<SchedulerDisplayPreferencesDto>(ApiRoutes.ScheduledJobs.GET_DISPLAY_PREFERENCES, Arg.Any<CancellationToken>());
    }
}
