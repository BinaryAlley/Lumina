#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
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
    private readonly UpdateUserSettingsRequestFixture _updateUserSettingsRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdateUserSettingsRequest_ShouldPreserveValues()
    {
        // Arrange
        UpdateUserSettingsRequest expected = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: 50,
            shouldIgnoreThePrefixForAlphaPicker: true,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

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
        UpdateUserSettingsRequest sut = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: false,
            itemsPerPage: 25,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: false,
            shouldAggregateMetadataWhenMissing: false,
            shouldRenderPdfAsImages: true,
            shouldPreserveBookStyles: true
        );

        // Act
        (bool isPaginationEnabled, int itemsPerPage, bool shouldIgnoreThePrefixForAlphaPicker, bool isThemeCachingEnabled, bool shouldAggregateMetadataWhenMissing, bool shouldRenderPdfAsImages, bool shouldPreserveBookStyles) = sut;

        // Assert
        Assert.Equal(sut.IsPaginationEnabled, isPaginationEnabled);
        Assert.Equal(sut.ItemsPerPage, itemsPerPage);
        Assert.Equal(sut.ShouldIgnoreThePrefixForAlphaPicker, shouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(sut.IsThemeCachingEnabled, isThemeCachingEnabled);
        Assert.Equal(sut.ShouldAggregateMetadataWhenMissing, shouldAggregateMetadataWhenMissing);
        Assert.Equal(sut.ShouldRenderPdfAsImages, shouldRenderPdfAsImages);
        Assert.Equal(sut.ShouldPreserveBookStyles, shouldPreserveBookStyles);
    }
}
