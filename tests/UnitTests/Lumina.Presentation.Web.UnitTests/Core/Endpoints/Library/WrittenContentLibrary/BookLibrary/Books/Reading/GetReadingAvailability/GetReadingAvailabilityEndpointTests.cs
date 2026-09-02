#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingAvailabilityEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetReadingAvailabilityEndpoint _sut;
    private readonly ReadingAvailabilityDtoFixture _readingAvailabilityDtoFixture = new();
    private readonly GetBookReadingAvailabilityRequestFixture _requestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpointTests"/> class.
    /// </summary>
    public GetReadingAvailabilityEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetReadingAvailabilityEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookIsAvailable_ShouldReturnSuccessJson()
    {
        // Arrange
        GetBookReadingAvailabilityRequest request = _requestFixture.Create();
        ReadingAvailabilityDto availability = _readingAvailabilityDtoFixture.Create(bookId: request.BookId, isAvailable: true);
        _mockApiHttpClient.GetAsync<ReadingAvailabilityDto>(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>())
            .Returns(availability);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(availability.LibraryId, jsonDocument.RootElement.GetProperty("libraryId").GetGuid());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookIsNotAvailable_ShouldReturnUnavailableJsonWithErrorCode()
    {
        // Arrange
        GetBookReadingAvailabilityRequest request = _requestFixture.Create();
        ReadingAvailabilityDto availability = _readingAvailabilityDtoFixture.Create(bookId: request.BookId, isAvailable: false, errorCode: "ReaderDisabled");
        _mockApiHttpClient.GetAsync<ReadingAvailabilityDto>(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>())
            .Returns(availability);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.False(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("ReaderDisabled", jsonDocument.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestReadingAvailabilityFromApi()
    {
        // Arrange
        GetBookReadingAvailabilityRequest request = new(BookId: Guid.NewGuid());
        ReadingAvailabilityDto availability = _readingAvailabilityDtoFixture.Create(bookId: request.BookId, isAvailable: true);
        _mockApiHttpClient.GetAsync<ReadingAvailabilityDto>(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>())
            .Returns(availability);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ReadingAvailabilityDto>(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>());
    }
}
