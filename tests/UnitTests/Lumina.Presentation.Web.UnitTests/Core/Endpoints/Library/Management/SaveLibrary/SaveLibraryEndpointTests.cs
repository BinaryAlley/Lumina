#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SaveLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.SaveLibrary;

/// <summary>
/// Contains unit tests for the <see cref="SaveLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveLibraryEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SaveLibraryEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveLibraryEndpointTests"/> class.
    /// </summary>
    public SaveLibraryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<SaveLibraryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasId_ShouldUpdateLibraryViaApi()
    {
        // Arrange
        LibraryDto request = _libraryDtoFixture.Create();
        LibraryDto expectedResponse = _libraryDtoFixture.Create(id: request.Id);
        _mockApiHttpClient.PutAsync<LibraryDto, LibraryDto>(Arg.Any<string>(), Arg.Any<LibraryDto>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.Libraries.UPDATE_LIBRARY.Replace("{id}", request.Id!.Value.ToString());
        await _mockApiHttpClient.Received(1).PutAsync<LibraryDto, LibraryDto>(expectedEndpoint, Arg.Is<LibraryDto>(library => library.Title == request.Title), Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasNoId_ShouldCreateLibraryViaApi()
    {
        // Arrange
        LibraryDto request = _libraryDtoFixture.Create();
        request.Id = null;
        LibraryDto expectedResponse = _libraryDtoFixture.Create(id: request.Id);
        _mockApiHttpClient.PostAsync<LibraryDto, LibraryDto>(Arg.Any<string>(), Arg.Any<LibraryDto>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<LibraryDto, LibraryDto>(ApiRoutes.Libraries.ADD_LIBRARY, Arg.Is<LibraryDto>(library => library.Title == request.Title), Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PutAsync<LibraryDto, LibraryDto>(Arg.Any<string>(), Arg.Any<LibraryDto>(), Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
