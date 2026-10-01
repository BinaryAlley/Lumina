#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzLabelInfoResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzLabelInfoResponseTests
{
    [Fact]
    public void CatalogNumberAndLabel_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"catalog-number":"EMC 4008","label":{"id":"f0e0e0e0-0000-0000-0000-000000000000","name":"EMI"}}""";

        // Act
        MusicBrainzLabelInfoResponse? response = JsonSerializer.Deserialize<MusicBrainzLabelInfoResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("EMC 4008", response.CatalogNumber);
        Assert.NotNull(response.Label);
        Assert.Equal("EMI", response.Label.Name);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "catalog-number": "EMC 4008",
            "label": { "id": "label-id", "name": "EMI" }
        }
        """;

        // Act
        MusicBrainzLabelInfoResponse? response = JsonSerializer.Deserialize<MusicBrainzLabelInfoResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("EMC 4008", response.CatalogNumber);
        Assert.NotNull(response.Label);
        Assert.Equal("EMI", response.Label.Name);
    }
}
