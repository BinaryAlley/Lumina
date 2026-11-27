#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Common;

/// <summary>
/// Contains unit tests for the <see cref="PaginatedResponse{TData}"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginatedResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_WhenDataIsMissing_ShouldThrowJsonException()
    {
        // Arrange
        string json = """{ "currentPage": 1, "perPage": 10, "count": 0, "numberOfPages": 0 }""";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PaginatedResponse<int>>(json, _jsonOptions));
    }
}
