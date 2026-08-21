#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
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
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateUserSettingsRequest request = new(IsPaginationEnabled: true, ItemsPerPage: 48, IgnoreThePrefixForAlphaPicker: false, IsThemeCachingEnabled: true);

        // Act
        UpdateUserSettingsCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(request.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(request.IgnoreThePrefixForAlphaPicker, result.IgnoreThePrefixForAlphaPicker);
        Assert.Equal(request.IsThemeCachingEnabled, result.IsThemeCachingEnabled);
    }
}
