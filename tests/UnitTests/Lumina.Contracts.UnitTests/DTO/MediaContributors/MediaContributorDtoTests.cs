#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaContributors;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingContributor_ShouldPreserveValues()
    {
        // Arrange
        MediaContributorDto expected = new(
            new MediaContributorNameDto("John Smith", "Johnathan Smith"),
            new MediaContributorRoleDto("Author", "Writer")
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaContributorDto? actual = JsonSerializer.Deserialize<MediaContributorDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingContributorWithoutNameOrRole_ShouldPreserveNullValues()
    {
        // Arrange
        MediaContributorDto expected = new(null, null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaContributorDto? actual = JsonSerializer.Deserialize<MediaContributorDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Name);
        Assert.Null(actual.Role);
    }
}
