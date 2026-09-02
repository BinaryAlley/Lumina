#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingAvailabilityRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityRequestMappingTests
{
    private readonly GetReadingAvailabilityRequestFixture _getReadingAvailabilityRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetReadingAvailabilityRequest request = _getReadingAvailabilityRequestFixture.Create();

        // Act
        GetReadingAvailabilityQuery result = request.ToQuery();

        // Assert
        Assert.Equal(request.BookId, result.BookId);
    }
}
