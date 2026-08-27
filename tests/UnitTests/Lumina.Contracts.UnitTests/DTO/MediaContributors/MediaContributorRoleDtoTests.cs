#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaContributors;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorRoleDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorRoleDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingRole_ShouldPreserveValues()
    {
        // Arrange
        MediaContributorRoleDto expected = new("Author", MediaContributorRoleCategory.Author);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaContributorRoleDto? actual = JsonSerializer.Deserialize<MediaContributorRoleDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        MediaContributorRoleDto first = new("Illustrator", null);
        MediaContributorRoleDto second = new("Illustrator", null);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
