#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzTextRepresentationResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzTextRepresentationResponseTests
{
    [Fact]
    public void LanguageAndScript_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"language":"eng","script":"Latn"}""";

        // Act
        MusicBrainzTextRepresentationResponse? response = JsonSerializer.Deserialize<MusicBrainzTextRepresentationResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("eng", response.Language);
        Assert.Equal("Latn", response.Script);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """{"language":"eng","script":"Latn"}""";

        // Act
        MusicBrainzTextRepresentationResponse? response = JsonSerializer.Deserialize<MusicBrainzTextRepresentationResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("eng", response.Language);
        Assert.Equal("Latn", response.Script);
    }
}
