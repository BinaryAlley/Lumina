#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.GetLibraries;

/// <summary>
/// Contains unit tests for the <see cref="GetLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetLibrariesEndpoint _sut;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpointTests"/> class.
    /// </summary>
    public GetLibrariesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetLibrariesEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsLibraries_ShouldReturnSuccessJsonWithLibraries()
    {
        // Arrange
        LibraryDto[] expectedLibraries = [.. _libraryDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryDto[]>(ApiRoutes.Libraries.GET_LIBRARIES, Arg.Any<CancellationToken>())
            .Returns(expectedLibraries);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        LibraryDto[]? returnedLibraries = jsonDocument.RootElement.GetProperty("data").Deserialize<LibraryDto[]>(_jsonOptions);
        Assert.Equal(expectedLibraries.Select(library => library.Title), returnedLibraries!.Select(library => library.Title));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestLibrariesFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<LibraryDto[]>(ApiRoutes.Libraries.GET_LIBRARIES, Arg.Any<CancellationToken>())
            .Returns([.. _libraryDtoFixture.CreateMany(2)]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryDto[]>(ApiRoutes.Libraries.GET_LIBRARIES, Arg.Any<CancellationToken>());
    }
}
