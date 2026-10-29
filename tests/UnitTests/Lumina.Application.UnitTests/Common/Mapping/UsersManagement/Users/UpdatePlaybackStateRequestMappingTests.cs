#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaybackStateRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateRequestMappingTests
{
    private readonly UpdatePlaybackStateRequestFixture _updatePlaybackStateRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.35, isMuted: true);

        // Act
        UpdatePlaybackStateCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.35, result.Volume);
        Assert.True(result.IsMuted);
    }

    [Fact]
    public void ToCommand_WhenRequestHasNullValues_ShouldPreserveTheNulls()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(includeVolume: false, includeIsMuted: false);

        // Act
        UpdatePlaybackStateCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Volume);
        Assert.Null(result.IsMuted);
    }

    [Theory]
    [InlineData(0.0, false)] // the minimum volume, not muted
    [InlineData(1.0, true)] // the maximum volume, muted
    public void ToCommand_WhenMappingBoundaryValues_ShouldMapThemExactly(double volume, bool isMuted)
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: volume, isMuted: isMuted);

        // Act
        UpdatePlaybackStateCommand result = request.ToCommand();

        // Assert
        Assert.Equal(volume, result.Volume);
        Assert.Equal(isMuted, result.IsMuted);
    }
}
