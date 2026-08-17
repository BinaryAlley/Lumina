#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Directories;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;
using Lumina.Presentation.Web.Fixtures.Common.Requests.FileSystemManagement.Directories;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetDirectoriesEndpoint _sut;
    private readonly GetDirectoriesRequestFixture _getDirectoriesRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    public GetDirectoriesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetDirectoriesEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsDirectories_ShouldReturnSuccessJsonWithDirectories()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create(path: "/media", includeHiddenElements: true);
        DirectoryDto[] expectedDirectories = [new DirectoryDto { Path = "/media/books", Name = "books" }];
        _mockApiHttpClient.GetAsync<DirectoryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedDirectories);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = $"{ApiRoutes.Directories.GET_DIRECTORIES}?path={Uri.EscapeDataString(request.Path!)}&includeHiddenElements=True";
        await _mockApiHttpClient.Received(1).GetAsync<DirectoryDto[]>(expectedEndpoint, Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(1, jsonDocument.RootElement.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithoutHiddenElements_ShouldRequestWithoutHiddenElementsFlag()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create(path: "/media", includeHiddenElements: false);
        _mockApiHttpClient.GetAsync<DirectoryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        string expectedEndpoint = $"{ApiRoutes.Directories.GET_DIRECTORIES}?path={Uri.EscapeDataString(request.Path!)}&includeHiddenElements=False";
        await _mockApiHttpClient.Received(1).GetAsync<DirectoryDto[]>(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
