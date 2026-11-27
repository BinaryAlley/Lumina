#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeBookReaders;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.GetLibraryTypeBookReaders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeBookReadersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetLibraryTypeBookReadersEndpoint _sut;
    private readonly GetLibraryTypeBookReadersRequestFixture _getLibraryTypeBookReadersRequestFixture = new();
    private readonly LibraryBookReaderDtoFixture _libraryBookReaderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersEndpointTests"/> class.
    /// </summary>
    public GetLibraryTypeBookReadersEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetLibraryTypeBookReadersEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithTheReaders()
    {
        // Arrange
        GetLibraryTypeBookReadersRequest request = _getLibraryTypeBookReadersRequestFixture.Create();
        LibraryBookReaderDto[] readers = [.. _libraryBookReaderDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryBookReaderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(readers);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(readers.Length, data.GetArrayLength());
        Assert.Equal(readers[0].PluginId, data[0].GetProperty("pluginId").GetGuid());
        Assert.Equal(readers[0].Name, data[0].GetProperty("name").GetString());
        Assert.Equal(readers[0].IsEnabled, data[0].GetProperty("isEnabled").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestTheReadersFromApiWithSubstitutedLibraryType()
    {
        // Arrange
        GetLibraryTypeBookReadersRequest request = _getLibraryTypeBookReadersRequestFixture.Create(libraryType: "Music");
        _mockApiHttpClient.GetAsync<LibraryBookReaderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryBookReaderDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_TYPE_BOOK_READERS.Replace("{libraryType}", "Music")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryTypeContainsPathCharacters_ShouldEscapeItInTheApiRoute()
    {
        // Arrange
        GetLibraryTypeBookReadersRequest request = _getLibraryTypeBookReadersRequestFixture.Create(libraryType: "../plugins");
        _mockApiHttpClient.GetAsync<LibraryBookReaderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryBookReaderDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_TYPE_BOOK_READERS.Replace("{libraryType}", Uri.EscapeDataString("../plugins"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardCancellationTokenToApi()
    {
        // Arrange
        GetLibraryTypeBookReadersRequest request = _getLibraryTypeBookReadersRequestFixture.Create();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockApiHttpClient.GetAsync<LibraryBookReaderDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryBookReaderDto[]>(
            Arg.Any<string>(),
            Arg.Is(cancellationToken));
    }
}
