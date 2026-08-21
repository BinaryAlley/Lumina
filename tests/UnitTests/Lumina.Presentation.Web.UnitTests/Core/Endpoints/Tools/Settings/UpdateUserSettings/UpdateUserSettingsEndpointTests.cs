#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdateUserSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Tools.Settings.UpdateUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly UpdateUserSettingsEndpoint _sut;
    private readonly UserSettingsDtoFixture _userSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointTests"/> class.
    /// </summary>
    public UpdateUserSettingsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<UpdateUserSettingsEndpoint>(_mockApiHttpClient, CreateThemeCachePreferenceService());
    }

    private static ThemeCachePreferenceService CreateThemeCachePreferenceService()
    {
        ServiceCollection services = new();
        services.AddHybridCache();
        return new ThemeCachePreferenceService(services.BuildServiceProvider().GetRequiredService<HybridCache>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldUpdateSettingsViaApiAndReturnSuccess()
    {
        // Arrange
        UserSettingsDto request = _userSettingsDtoFixture.Create();
        _mockApiHttpClient.PutAsync<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest, UserSettingsDto>(Arg.Any<string>(), Arg.Any<UserSettingsDto>(), Arg.Any<CancellationToken>())
            .Returns(new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest, UserSettingsDto>(
            ApiRoutes.Users.UPDATE_USER_SETTINGS,
            Arg.Is<UserSettingsDto>(settings => settings.ItemsPerPage == request.ItemsPerPage && settings.IsPaginationEnabled == request.IsPaginationEnabled),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isUpdated").GetBoolean());
    }
}
