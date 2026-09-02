#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsRequestMappingTests
{
    private readonly UpdateUserSettingsRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateUserSettingsRequest request = _requestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: 48,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false,
            shouldRenderPdfAsImages: true,
            shouldPreserveBookStyles: true
        );

        // Act
        UpdateUserSettingsCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(request.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(request.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(request.IsThemeCachingEnabled, result.IsThemeCachingEnabled);
        Assert.Equal(request.ShouldRenderPdfAsImages, result.ShouldRenderPdfAsImages);
        Assert.Equal(request.ShouldPreserveBookStyles, result.ShouldPreserveBookStyles);
    }
}
