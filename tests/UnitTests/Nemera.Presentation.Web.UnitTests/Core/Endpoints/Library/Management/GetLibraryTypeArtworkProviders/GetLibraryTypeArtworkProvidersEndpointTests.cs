#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeArtworkProviders;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.GetLibraryTypeArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeArtworkProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetLibraryTypeArtworkProvidersEndpoint _sut;
    private readonly GetLibraryTypeArtworkProvidersRequestFixture _getLibraryTypeArtworkProvidersRequestFixture = new();
    private readonly LibraryArtworkProviderDtoFixture _libraryArtworkProviderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersEndpointTests"/> class.
    /// </summary>
    public GetLibraryTypeArtworkProvidersEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetLibraryTypeArtworkProvidersEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithTheProviders()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create();
        LibraryArtworkProviderDto[] providers = [.. _libraryArtworkProviderDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryArtworkProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(providers);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(providers.Length, data.GetArrayLength());
        Assert.Equal(providers[0].PluginId, data[0].GetProperty("pluginId").GetGuid());
        Assert.Equal(providers[0].Name, data[0].GetProperty("name").GetString());
        Assert.Equal(providers[0].IsEnabled, data[0].GetProperty("isEnabled").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestTheProvidersFromApiWithSubstitutedLibraryType()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create(libraryType: "Music");
        _mockApiHttpClient.GetAsync<LibraryArtworkProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryArtworkProviderDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_TYPE_ARTWORK_PROVIDERS.Replace("{libraryType}", "Music")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryTypeContainsPathCharacters_ShouldEscapeItInTheApiRoute()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create(libraryType: "../plugins");
        _mockApiHttpClient.GetAsync<LibraryArtworkProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryArtworkProviderDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_TYPE_ARTWORK_PROVIDERS.Replace("{libraryType}", Uri.EscapeDataString("../plugins"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardCancellationTokenToApi()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockApiHttpClient.GetAsync<LibraryArtworkProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryArtworkProviderDto[]>(
            Arg.Any<string>(),
            Arg.Is(cancellationToken));
    }
}
