#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Common;

/// <summary>
/// Contains unit tests for the <see cref="TagDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingTag_ShouldPreserveName()
    {
        // Arrange
        TagDto expected = new("bestseller");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        TagDto? actual = JsonSerializer.Deserialize<TagDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        TagDto first = new("award-winning");
        TagDto second = new("award-winning");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
