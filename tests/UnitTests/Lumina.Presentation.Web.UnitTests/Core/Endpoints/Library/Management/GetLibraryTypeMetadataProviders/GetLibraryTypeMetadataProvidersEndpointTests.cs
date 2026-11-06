#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeMetadataProviders;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.GetLibraryTypeMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetLibraryTypeMetadataProvidersEndpoint _sut;
    private readonly GetLibraryTypeMetadataProvidersRequestFixture _getLibraryTypeMetadataProvidersRequestFixture = new();
    private readonly LibraryMetadataProviderDtoFixture _libraryMetadataProviderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersEndpointTests"/> class.
    /// </summary>
    public GetLibraryTypeMetadataProvidersEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetLibraryTypeMetadataProvidersEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithTheProviders()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create();
        LibraryMetadataProviderDto[] providers = [.. _libraryMetadataProviderDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryMetadataProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
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
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create(libraryType: "Music");
        _mockApiHttpClient.GetAsync<LibraryMetadataProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryMetadataProviderDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_TYPE_METADATA_PROVIDERS.Replace("{libraryType}", "Music")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryTypeContainsPathCharacters_ShouldEscapeItInTheApiRoute()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create(libraryType: "../plugins");
        _mockApiHttpClient.GetAsync<LibraryMetadataProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryMetadataProviderDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_TYPE_METADATA_PROVIDERS.Replace("{libraryType}", Uri.EscapeDataString("../plugins"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardCancellationTokenToApi()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockApiHttpClient.GetAsync<LibraryMetadataProviderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryMetadataProviderDto[]>(
            Arg.Any<string>(),
            Arg.Is(cancellationToken));
    }
}
