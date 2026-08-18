#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.UsersManagement.Settings;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdateUserSettingsRequest_ShouldPreserveValues()
    {
        // Arrange
        UpdateUserSettingsRequest expected = new(true, 50, true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateUserSettingsRequest? actual = JsonSerializer.Deserialize<UpdateUserSettingsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        UpdateUserSettingsRequest sut = new(false, 25, false);

        // Act
        (bool isPaginationEnabled, int itemsPerPage, bool ignoreThePrefixForAlphaPicker) = sut;

        // Assert
        Assert.Equal(sut.IsPaginationEnabled, isPaginationEnabled);
        Assert.Equal(sut.ItemsPerPage, itemsPerPage);
        Assert.Equal(sut.IgnoreThePrefixForAlphaPicker, ignoreThePrefixForAlphaPicker);
    }
}
