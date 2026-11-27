#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.Plugins;
using Nemera.Presentation.Web.Common.Requests.Library.Management;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Library.Management.GetBookReaders;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Library.Management;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.GetBookReaders;

/// <summary>
/// Contains unit tests for the <see cref="GetBookReadersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadersEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetBookReadersEndpoint _sut;
    private readonly GetBookReadersRequestFixture _getBookReadersRequestFixture = new();
    private readonly LibraryBookReaderDtoFixture _libraryBookReaderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookReadersEndpointTests"/> class.
    /// </summary>
    public GetBookReadersEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetBookReadersEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithBookReaders()
    {
        // Arrange
        GetBookReadersRequest request = _getBookReadersRequestFixture.Create();
        LibraryBookReaderDto[] readers = [.. _libraryBookReaderDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryBookReaderDto[]>(ApiRoutes.Libraries.GET_LIBRARY_BOOK_READERS.Replace("{libraryId}", request.LibraryId.ToString()), Arg.Any<CancellationToken>())
            .Returns(readers);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(readers.Length, jsonDocument.RootElement.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestBookReadersFromApi()
    {
        // Arrange
        GetBookReadersRequest request = _getBookReadersRequestFixture.Create();
        LibraryBookReaderDto[] readers = [.. _libraryBookReaderDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<LibraryBookReaderDto[]>(ApiRoutes.Libraries.GET_LIBRARY_BOOK_READERS.Replace("{libraryId}", request.LibraryId.ToString()), Arg.Any<CancellationToken>())
            .Returns(readers);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryBookReaderDto[]>(ApiRoutes.Libraries.GET_LIBRARY_BOOK_READERS.Replace("{libraryId}", request.LibraryId.ToString()), Arg.Any<CancellationToken>());
    }
}
