#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetUserSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Tools.Settings.GetUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="GetUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetUserSettingsEndpoint _sut;
    private readonly UserSettingsDtoFixture _userSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointTests"/> class.
    /// </summary>
    public GetUserSettingsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetUserSettingsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsSettings_ShouldReturnSuccessJsonWithSettings()
    {
        // Arrange
        UserSettingsDto expectedSettings = _userSettingsDtoFixture.Create();
        _mockApiHttpClient.GetAsync<UserSettingsDto>(ApiRoutes.Users.GET_USER_SETTINGS, Arg.Any<CancellationToken>())
            .Returns(expectedSettings);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(expectedSettings.ItemsPerPage, data.GetProperty("itemsPerPage").GetInt32());
        Assert.Equal(expectedSettings.IsPaginationEnabled, data.GetProperty("isPaginationEnabled").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestUserSettingsFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<UserSettingsDto>(ApiRoutes.Users.GET_USER_SETTINGS, Arg.Any<CancellationToken>())
            .Returns(_userSettingsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<UserSettingsDto>(ApiRoutes.Users.GET_USER_SETTINGS, Arg.Any<CancellationToken>());
    }
}
