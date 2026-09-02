#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetReadingSectionEndpoint _sut;
    private readonly GetBookReadingSectionRequestFixture _requestFixture = new();
    private readonly ReadingSectionDtoFixture _readingSectionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpointTests"/> class.
    /// </summary>
    public GetReadingSectionEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetReadingSectionEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithSection()
    {
        // Arrange
        GetBookReadingSectionRequest request = _requestFixture.Create();
        ReadingSectionDto section = _readingSectionDtoFixture.Create(locationRef: request.LocationRef);
        _mockApiHttpClient.GetAsync<ReadingSectionDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(section);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(section.LocationRef, jsonDocument.RootElement.GetProperty("data").GetProperty("locationRef").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestReadingSectionFromApi()
    {
        // Arrange
        GetBookReadingSectionRequest request = _requestFixture.Create();
        ReadingSectionDto section = _readingSectionDtoFixture.Create(locationRef: request.LocationRef);
        _mockApiHttpClient.GetAsync<ReadingSectionDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(section);
        string expectedEndpoint = ApiRoutes.Books.GET_BOOK_READING_SECTION
            .Replace("{bookId}", request.BookId.ToString())
            .Replace("{locationRef}", Uri.EscapeDataString(request.LocationRef));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ReadingSectionDto>(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
