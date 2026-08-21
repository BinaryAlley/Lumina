#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Services;

/// <summary>
/// Contains unit tests for the <see cref="ThemeCachePreferenceService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeCachePreferenceServiceTests
{
    private readonly HybridCache _hybridCache;
    private readonly ThemeCachePreferenceService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeCachePreferenceService"/> class.
    /// </summary>
    public ThemeCachePreferenceServiceTests()
    {
        ServiceCollection services = new();
        services.AddHybridCache();
        _hybridCache = services.BuildServiceProvider().GetRequiredService<HybridCache>();
        _sut = new ThemeCachePreferenceService(_hybridCache);
    }

    [Fact]
    public async Task GetAsync_WhenNoPreferenceStored_ShouldReturnDefaultValue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        bool result = await _sut.GetAsync(userId, defaultValue: true, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAsync_WhenNoPreferenceStoredAndDefaultIsFalse_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        bool result = await _sut.GetAsync(userId, defaultValue: false, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetAsync_WhenPreferenceStored_ShouldReturnTheStoredValue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        await _sut.SetAsync(userId, isEnabled: true, CancellationToken.None);
        bool result = await _sut.GetAsync(userId, defaultValue: false, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetAsync_WhenPreferenceOverwritten_ShouldReturnTheNewValue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        await _sut.SetAsync(userId, isEnabled: true, CancellationToken.None);
        await _sut.SetAsync(userId, isEnabled: false, CancellationToken.None);
        bool result = await _sut.GetAsync(userId, defaultValue: true, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetAsync_WhenStoredForOneUser_ShouldNotAffectAnotherUser()
    {
        // Arrange
        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();

        // Act
        await _sut.SetAsync(firstUserId, isEnabled: true, CancellationToken.None);
        bool firstUserResult = await _sut.GetAsync(firstUserId, defaultValue: false, CancellationToken.None);
        bool secondUserResult = await _sut.GetAsync(secondUserId, defaultValue: false, CancellationToken.None);

        // Assert
        Assert.True(firstUserResult);
        Assert.False(secondUserResult);
    }
}
